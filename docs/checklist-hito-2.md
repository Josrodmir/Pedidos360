# Checklist de pruebas — Hito 2

| Caso | Resultado esperado |
|---|---|
| Inicio de sesión | Una persona autenticada puede acceder al catálogo según su rol. |
| Roles | `Administrador` administra usuarios y catálogo; `Ventas` administra clientes y confirma pedidos; `Inventario` administra catálogo; `Contabilidad` consulta pedidos. |
| Filtros AJAX | Al escribir en el catálogo o cambiar una categoría, los productos se actualizan sin recargar la página. |
| Búsqueda AJAX | Al escribir en el formulario de pedido se consultan productos activos con stock, sin recargar la página. |
| Cálculo AJAX | Cantidad y descuento actualizan subtotal, impuestos y total sin recargar la página. |
| Confirmación | Un pedido válido guarda cliente, usuario, fecha, detalle y totales; además descuenta el stock. |
| Validación de inventario | No se confirma un pedido si un producto está inactivo, no existe o no tiene unidades suficientes. |
| Datos únicos | No se permiten dos clientes con la misma identificación. |
| Errores | Si se excede el stock, se muestra un mensaje de negocio y no se registra el pedido; las rutas inexistentes y errores 500 muestran una respuesta controlada. |

## Preparación de la demostración

1. Aplique las migraciones con `Update-Database` contra `Pedidos360DB`.
2. Inicie la aplicación en `https://localhost:7111`.
3. Registre el primer usuario y asígnele `Administrador` con `docs/asignar-administrador-inicial.sql`. Después use **Usuarios** para asignar `Ventas`, `Inventario` o `Contabilidad`.
4. Cree al menos un cliente y productos activos con stock antes de demostrar pedidos.
