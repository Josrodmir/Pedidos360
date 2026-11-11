# Guía para el caso de estudio — Pedidos360

## Objetivo de la demostración

Mostrar que Pedidos360 resuelve un flujo B2B completo: catálogo, clientes, pedidos, inventario, roles y persistencia en Azure SQL con validaciones y llamadas AJAX.

## Antes de iniciar

1. Abrir `Pedidos360.slnx` en Visual Studio.
2. Configurar `DefaultConnection` en User Secrets con la cadena de Azure SQL o usar LocalDB.
3. Ejecutar `Update-Database` desde la Consola del Administrador de Paquetes.
4. Ejecutar el proyecto (`F5`) y comprobar que carga la pantalla de inicio.
5. Tener preparada una imagen JPG, PNG o WEBP de menos de 2 MB para la demostración de productos.

> No incluir contraseñas ni cadenas de conexión con credenciales en diapositivas, repositorios o capturas.

## Recorrido recomendado (5–7 minutos)

1. **Contexto (30 s):** presentar el problema: una pyme B2B necesita controlar catálogo, clientes e inventario antes de registrar pedidos.
2. **Modelo de datos (45 s):** abrir `docs/diagrama-entidad-relacion-final.svg`. Explicar Categoría, Producto, Cliente, Pedido, PedidoDetalle e Identity.
3. **Catálogo y AJAX (1 min):** filtrar por texto y categoría; el resultado cambia sin recargar. Explicar que `ApiProductosController` atiende la solicitud.
4. **Clientes (1 min):** crear un cliente y demostrar las validaciones de correo, cédula única y datos obligatorios.
5. **Pedido y errores de negocio (2 min):** buscar productos con AJAX, agregar líneas y mostrar los totales en vivo. Intentar exceder el stock: la aplicación rechaza el pedido y muestra el mensaje sin modificar el inventario.
6. **Seguridad y persistencia (1 min):** mostrar los roles Administrador, Ventas, Inventario y Contabilidad; explicar que las contraseñas se almacenan como hashes. Señalar migraciones, Azure SQL, reintentos y calentamiento serverless.
7. **Cierre (30 s):** mostrar Story Map y relacionar cada requisito solicitado con la función terminada.

## Preguntas frecuentes y respuestas breves

| Pregunta | Respuesta |
|---|---|
| ¿Por qué EF Core Code First? | Porque el modelo y las migraciones quedan versionados junto con el código y el esquema se puede reproducir. |
| ¿Dónde se guardan las imágenes? | En la tabla de productos como bytes (`varbinary(max)`), con su tipo MIME; no se depende de una ruta local. |
| ¿Cómo se evita información incorrecta? | Con anotaciones y validación de servidor, además de restricciones en base de datos: categorías válidas y cédula única. |
| ¿Qué ocurre si Azure SQL tarda en responder? | EF Core reintenta fallos transitorios, aumenta el tiempo de espera y un servicio realiza calentamiento al iniciar. |
| ¿Cómo se protegen las cuentas? | ASP.NET Identity guarda hashes de contraseñas y relaciona cada usuario con un rol en `AspNetUserRoles`. |

## Evidencias a mostrar

- `docs/story-map.md`: alcance, historias y criterios de aceptación.
- `docs/diagrama-entidad-relacion-final.svg`: modelo de datos.
- `Data/Migrations/`: evolución reproducible del esquema.
- CRUD de Productos y Clientes, filtros AJAX y creación de pedidos en ejecución.
- `ApiProductosController` y `ApiPedidosController` como evidencia de Web API.
- Pantalla **Usuarios** como evidencia de autorización por roles.
- Imagen cargada desde la base de datos en el detalle del producto.

## Lista de verificación final

- [ ] La conexión apunta a la base de datos disponible para la clase.
- [ ] Las migraciones están aplicadas.
- [ ] Hay al menos una categoría y un producto para mostrar filtros y búsqueda.
- [ ] Hay un cliente de ejemplo para mostrar el CRUD.
- [ ] Se creó un pedido de prueba y hay suficiente stock para una prueba válida.
- [ ] Se probó un pedido que excede el stock para mostrar el error de negocio.
- [ ] La presentación técnica, el ERD, Story Map y esta guía están abiertos antes de iniciar.
- [ ] No se muestran secretos, contraseñas ni datos personales reales.
