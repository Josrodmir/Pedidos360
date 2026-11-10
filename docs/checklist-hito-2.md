# Checklist de pruebas — Hito 2

| Caso | Resultado esperado |
|---|---|
| Inicio de sesión | Una persona autenticada puede acceder al catálogo según su rol. |
| Roles | `Admin` administra el catálogo completo; `Ventas` administra clientes y confirma pedidos; `Operaciones` consulta catálogo y pedidos. |
| Búsqueda AJAX | Al escribir en el formulario de pedido se consultan productos activos con stock, sin recargar la página. |
| Cálculo AJAX | Cantidad y descuento actualizan subtotal, impuestos y total sin recargar la página. |
| Confirmación | Un pedido válido guarda cliente, usuario, fecha, detalle y totales; además descuenta el stock. |
| Validación de inventario | No se confirma un pedido si un producto está inactivo, no existe o no tiene unidades suficientes. |
| Datos únicos | No se permiten dos clientes con la misma identificación. |
| Errores | Las rutas inexistentes muestran una respuesta controlada; una pausa de Azure SQL muestra una pantalla para reintentar. |

## Preparación de la demostración

1. Aplique las migraciones con `Update-Database` contra `Pedidos360DB`.
2. Inicie la aplicación en `https://localhost:7111`.
3. Registre usuarios y asígneles los roles `Admin`, `Ventas` u `Operaciones` desde la administración de Identity.
4. Cree al menos un cliente y productos activos con stock antes de demostrar pedidos.
