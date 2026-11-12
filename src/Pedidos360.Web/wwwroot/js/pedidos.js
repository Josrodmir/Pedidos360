(() => {
    const search = document.querySelector("#buscarProducto");
    const results = document.querySelector("#resultados");
    const lines = document.querySelector("#lineas");
    const totals = document.querySelector("#totales");
    const orderForm = document.querySelector("#pedidoForm");
    const confirmButton = orderForm?.querySelector("button[type='submit'], button:not([type])");
    let timer;
    let calculationController;

    search?.addEventListener("input", () => {
        clearTimeout(timer);
        timer = setTimeout(searchProducts, 250);
    });

    async function searchProducts() {
        const response = await fetch(`/api/productos/buscar?q=${encodeURIComponent(search.value)}`);
        if (!response.ok) {
            results.innerHTML = '<div class="list-group-item text-muted">No se pudo consultar el catálogo.</div>';
            return;
        }

        const products = await response.json();
        results.innerHTML = products.map(product =>
            `<button type="button" class="list-group-item" data-id="${product.id}" data-name="${product.nombre}" data-stock="${product.stock}">
                ${product.nombre} · ₡${product.precio.toFixed(2)} · stock ${product.stock}
             </button>`).join("");
    }

    results?.addEventListener("click", event => {
        const button = event.target.closest("button");
        if (!button || lines.querySelector(`[data-id="${button.dataset.id}"]`)) return;

        lines.insertAdjacentHTML("beforeend", `
            <tr data-id="${button.dataset.id}" data-name="${button.dataset.name}" data-stock="${button.dataset.stock}">
                <td>${button.dataset.name}<input type="hidden" value="${button.dataset.id}"></td>
                <td><input class="form-control calc" type="number" min="1" max="${button.dataset.stock}" value="1"></td>
                <td><input class="form-control calc" type="number" min="0" max="100" value="0"></td>
                <td><button type="button" class="btn btn-sm btn-outline-danger quitar" aria-label="Quitar producto">×</button></td>
            </tr>`);
        results.innerHTML = "";
        search.value = "";
        reindexLines();
        calculate();
    });

    lines?.addEventListener("input", calculate);
    lines?.addEventListener("click", event => {
        if (!event.target.classList.contains("quitar")) return;
        event.target.closest("tr").remove();
        reindexLines();
        calculate();
    });

    orderForm?.addEventListener("submit", event => {
        if (!lines.children.length || confirmButton?.disabled || totals.classList.contains("text-danger")) {
            event.preventDefault();
            if (!lines.children.length) showCalculationError("Agregue al menos un producto antes de confirmar.");
        }
    });

    function reindexLines() {
        [...lines.children].forEach((row, index) => {
            const [productId, quantity, discount] = row.querySelectorAll("input");
            productId.name = `Lineas[${index}].ProductoId`;
            quantity.name = `Lineas[${index}].Cantidad`;
            discount.name = `Lineas[${index}].Descuento`;
        });
    }

    async function calculate() {
        const orderLines = [...lines.children].map(row => {
            const [productId, quantity, discount] = row.querySelectorAll("input");
            return { productoId: +productId.value, cantidad: +quantity.value, descuento: +discount.value };
        });

        if (!orderLines.length) {
            totals.textContent = "Total: ₡0.00";
            totals.classList.remove("text-danger", "text-muted");
            setConfirmEnabled(false);
            return;
        }

        const stockError = getStockValidationError();
        if (stockError) {
            calculationController?.abort();
            showCalculationError(stockError);
            return;
        }

        calculationController?.abort();
        const controller = new AbortController();
        calculationController = controller;
        totals.classList.remove("text-danger");
        totals.classList.add("text-muted");
        totals.textContent = "Calculando totales…";
        setConfirmEnabled(false);

        try {
            const response = await fetch("/api/pedidos/calcular", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(orderLines),
                signal: controller.signal
            });

            if (controller !== calculationController) return;

            if (!response.ok) {
                showCalculationError(await getCalculationErrorMessage(response));
                return;
            }

            const data = await response.json();
            totals.classList.remove("text-danger", "text-muted");
            totals.textContent = `Subtotal: ₡${data.subtotal.toFixed(2)} · Impuestos: ₡${data.impuestos.toFixed(2)} · Total: ₡${data.total.toFixed(2)}`;
            setConfirmEnabled(true);
        } catch (error) {
            if (error.name === "AbortError" || controller !== calculationController) return;
            showCalculationError("No se pudieron calcular los totales. Intente nuevamente.");
        }
    }

    function getStockValidationError() {
        for (const row of lines.children) {
            const quantity = Number(row.querySelector(".calc").value);
            const stock = Number(row.dataset.stock);
            if (Number.isFinite(stock) && quantity > stock) {
                return `Stock insuficiente para ${row.dataset.name}. Disponibles: ${stock}.`;
            }
        }
        return null;
    }

    function showCalculationError(message) {
        totals.textContent = message;
        totals.classList.remove("text-muted");
        totals.classList.add("text-danger");
        setConfirmEnabled(false);
    }

    function setConfirmEnabled(enabled) {
        if (confirmButton) confirmButton.disabled = !enabled;
    }

    async function getCalculationErrorMessage(response) {
        const fallback = "No se pudieron calcular los totales. Revise las cantidades y descuentos.";
        const contentType = response.headers.get("content-type") || "";

        if (!contentType.includes("json")) {
            if (response.status === 401 || response.status === 403) {
                return "Su sesión no tiene permiso para calcular el pedido.";
            }
            if (response.status >= 500) {
                return "No fue posible calcular los totales en este momento. Intente nuevamente.";
            }
            return fallback;
        }

        let problem;
        try {
            problem = await response.json();
        } catch {
            return fallback;
        }
        if (problem.errors) {
            const fields = Object.keys(problem.errors).join(" ").toLowerCase();
            if (fields.includes("cantidad")) {
                return "La cantidad debe ser mayor que cero.";
            }
            if (fields.includes("descuento")) {
                return "El descuento debe ser un porcentaje entre 0 y 100.";
            }
        }

        return problem.detail || problem.title || fallback;
    }
})();
