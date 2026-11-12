(() => {
    const search = document.querySelector("#buscarProducto");
    const results = document.querySelector("#resultados");
    const lines = document.querySelector("#lineas");
    const totals = document.querySelector("#totales");
    let timer;

    search?.addEventListener("input", () => {
        clearTimeout(timer);
        timer = setTimeout(searchProducts, 250);
    });

    // Al enfocar el control mostramos las opciones disponibles. Así conserva la
    // búsqueda AJAX, pero también funciona visualmente como un desplegable.
    search?.addEventListener("focus", () => {
        if (!results.children.length) searchProducts();
    });

    async function searchProducts() {
        const response = await fetch(`/api/productos/buscar?q=${encodeURIComponent(search.value)}`);
        if (!response.ok) {
            results.innerHTML = '<div class="list-group-item text-muted">No se pudo consultar el catálogo.</div>';
            return;
        }

        const products = await response.json();
        results.innerHTML = products.length
            ? products.map(product =>
            `<button type="button" class="list-group-item" data-id="${product.id}" data-name="${product.nombre}">
                ${product.nombre} · ₡${product.precio.toFixed(2)} · stock ${product.stock}
             </button>`).join("")
            : '<div class="list-group-item text-muted">No hay productos disponibles con existencias.</div>';
    }

    results?.addEventListener("click", event => {
        const button = event.target.closest("button");
        if (!button || lines.querySelector(`[data-id="${button.dataset.id}"]`)) return;

        lines.insertAdjacentHTML("beforeend", `
            <tr data-id="${button.dataset.id}">
                <td>${button.dataset.name}<input type="hidden" value="${button.dataset.id}"></td>
                <td><input class="form-control calc" type="number" min="1" value="1"></td>
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
            return;
        }

        const response = await fetch("/api/pedidos/calcular", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(orderLines)
        });

        if (!response.ok) {
            const message = await response.text();
            totals.textContent = message || "No se pudieron calcular los totales. Revise las cantidades.";
            totals.classList.add("text-danger");
            return;
        }

        const data = await response.json();
        totals.classList.remove("text-danger");
        totals.textContent = `Subtotal: ₡${data.subtotal.toFixed(2)} · Impuestos: ₡${data.impuestos.toFixed(2)} · Total: ₡${data.total.toFixed(2)}`;
    }
})();
