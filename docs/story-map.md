# Story Map — Pedidos360

## Objetivo del producto

Permitir a una pyme B2B administrar su catálogo e inventario, mantener clientes y crear pedidos con totales auditables, control de stock y seguridad por roles.

## Leyenda

- ✅ **Implementado:** disponible en la aplicación y persistido en Azure SQL.
- 🟡 **Siguiente hito:** necesario para el MVP final.
- ⚪ **Mejora:** aporta valor, pero puede priorizarse después del MVP.

## Mapa de historias de usuario

| Actividad del usuario | Explorar | Gestionar | Validar / confirmar | Resultado esperado |
|---|---|---|---|---|
| **1. Acceder al sistema** | ✅ Ver pantalla de inicio | ✅ Registrarse / iniciar sesión | ✅ Aplicar rol | Cada usuario entra con permisos correctos. |
| **2. Administrar catálogo** | ✅ Buscar por nombre y filtrar por categoría | ✅ Crear, editar, consultar y eliminar productos | ✅ Validar precio, impuesto, stock, categoría e imagen | Catálogo actualizado y disponible para pedidos. |
| **3. Administrar imágenes** | ✅ Ver imagen de un producto | ✅ Cargar JPG, PNG o WEBP | ✅ Validar tipo y máximo 2 MB; guardar bytes en Azure SQL | La imagen queda asociada al producto como `varbinary(max)`. |
| **4. Administrar clientes** | ✅ Buscar por nombre o cédula | ✅ Crear, editar, consultar y eliminar clientes | ✅ Validar correo y cédula única | Clientes confiables para asignar pedidos. |
| **5. Crear pedido** | ✅ Seleccionar cliente | ✅ Buscar productos por nombre con AJAX | ✅ Agregar líneas, cantidades y descuentos | Pedido con productos correctos. |
| **6. Calcular y confirmar** | ✅ Ver subtotal, impuestos, descuentos y total | ✅ Modificar líneas del pedido | ✅ Confirmar y registrar | Pedido almacenado con valores auditables. |
| **7. Controlar inventario** | ✅ Consultar stock disponible | ✅ Disminuir stock al confirmar pedido | ✅ Bloquear cantidades superiores al stock | Inventario consistente. |
| **8. Consultar pedidos** | ✅ Listar pedidos | ✅ Abrir detalle | ⚪ Exportar PDF / Excel | Historial comercial consultable. |
| **9. Seguridad y administración** | ✅ Ver usuarios y roles | ✅ Asignar Administrador, Ventas, Contabilidad e Inventario | ✅ Restringir acciones según rol | Acceso controlado a funciones sensibles. |
| **10. Operación técnica** | ✅ Usar Azure SQL | ✅ Ejecutar migraciones con EF Core | ✅ Reintentos y calentamiento de Azure SQL Serverless | Aplicación estable y esquema reproducible. |

## Corte por hitos

### Hito 1 — Base funcional implementada ✅

| Entrega vertical | Evidencia |
|---|---|
| Catálogo de productos | CRUD, categoría, búsqueda, filtro, paginación y validaciones. |
| Clientes | CRUD, búsqueda, validación de correo y cédula única. |
| Persistencia | EF Core Code First, migraciones y Azure SQL `Pedidos360DB`. |
| Imágenes | Carga de JPG/PNG/WEBP, validación y almacenamiento binario. |
| Resiliencia | Reintentos de Azure SQL y consulta de calentamiento `SELECT 1` al iniciar. |

### Hito 2 — MVP de pedidos implementado ✅

| Prioridad | Historia de usuario | Criterio de aceptación |
|---|---|---|
| Alta | Como **Ventas**, quiero seleccionar un cliente para iniciar un pedido. | No se puede confirmar un pedido sin cliente válido. |
| Alta | Como **Ventas**, quiero buscar y agregar productos con AJAX. | Solo se muestran productos activos; la búsqueda responde sin recargar la página. |
| Alta | Como **Ventas**, quiero indicar cantidad y descuento por línea. | Cantidad mayor a 0 y no superior al stock disponible. |
| Alta | Como **Ventas**, quiero ver subtotal, impuestos y total en tiempo real. | Los cálculos coinciden con los valores almacenados al confirmar. |
| Alta | Como **Inventario**, quiero que el stock disminuya al confirmar. | Pedido, detalle y disminución de stock se guardan en una única operación de EF Core. |
| Media | Como **usuario autorizado**, quiero ver el detalle e historial de pedidos. | Se puede consultar cada línea, estado, usuario y total. |

### Hito 3 — Mejoras opcionales ⚪

| Prioridad | Historia de usuario | Criterio de aceptación |
|---|---|---|
| Alta | Como **Administrador**, quiero asignar roles a usuarios. | Administrador, Ventas, Contabilidad e Inventario ven únicamente sus acciones autorizadas. |
| Media | Como **usuario**, quiero manejar errores amigables. | Fallos de datos o red muestran un mensaje recuperable, sin exponer detalles técnicos. |
| Media | Como **Ventas**, quiero exportar un pedido. | Se genera un PDF o Excel con encabezado, líneas y totales. |
| Baja | Como **líder**, quiero indicadores del negocio. | Panel con pedidos, ventas, stock bajo y clientes activos. |

## Relación con el modelo de datos final

El Hito 2 incorpora `Pedidos` y `PedidoDetalles`. Cada pedido pertenece a un `Cliente` y a un usuario de `AspNetUsers`; cada línea referencia un `Producto`. La confirmación debe ejecutarse en una transacción para persistir el pedido, sus detalles y la disminución de stock de forma consistente.
