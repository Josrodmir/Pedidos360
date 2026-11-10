# Guía para el caso de estudio — Pedidos360

## Objetivo de la demostración

Mostrar que Pedidos360 resuelve el primer corte vertical de una aplicación B2B: administra catálogo y clientes, conserva los datos en SQL Server/Azure SQL y protege la calidad de los datos mediante validaciones.

## Antes de iniciar

1. Abrir `Pedidos360.slnx` en Visual Studio.
2. Configurar `DefaultConnection` en User Secrets con la cadena de Azure SQL o usar LocalDB.
3. Ejecutar `Update-Database` desde la Consola del Administrador de Paquetes.
4. Ejecutar el proyecto (`F5`) y comprobar que carga la pantalla de inicio.
5. Tener preparada una imagen JPG, PNG o WEBP de menos de 2 MB para la demostración de productos.

> No incluir contraseñas ni cadenas de conexión con credenciales en diapositivas, repositorios o capturas.

## Recorrido recomendado (5–7 minutos)

1. **Contexto (30 s):** presentar el problema: una pyme B2B necesita controlar catálogo, clientes e inventario antes de registrar pedidos.
2. **Modelo de datos (45 s):** abrir `docs/diagrama-entidad-relacion-final.svg`. Explicar las entidades Categoría, Producto y Cliente; destacar que un producto pertenece a una categoría y que la cédula de cliente es única.
3. **Productos (2 min):** crear un producto con categoría, precio, impuesto, stock e imagen. Mostrar la validación si se intenta una imagen de formato no permitido o superior a 2 MB. Buscarlo, filtrar por categoría y abrir su detalle.
4. **Clientes (1–2 min):** crear un cliente y mostrar las validaciones de correo y cédula. Probar una cédula repetida para explicar la restricción de unicidad.
5. **Persistencia y resiliencia (1 min):** señalar las migraciones de Entity Framework y la conexión a Azure SQL. Explicar que la aplicación usa reintentos y una consulta de calentamiento para Azure SQL Serverless.
6. **Cierre (30 s):** mostrar el Story Map y ubicar el estado actual: Hito 1 terminado; pedidos, disminución transaccional de stock y roles están planificados para el Hito 2.

## Preguntas frecuentes y respuestas breves

| Pregunta | Respuesta |
|---|---|
| ¿Por qué EF Core Code First? | Porque el modelo y las migraciones quedan versionados junto con el código y el esquema se puede reproducir. |
| ¿Dónde se guardan las imágenes? | En la tabla de productos como bytes (`varbinary(max)`), con su tipo MIME; no se depende de una ruta local. |
| ¿Cómo se evita información incorrecta? | Con anotaciones y validación de servidor, además de restricciones en base de datos: categorías válidas y cédula única. |
| ¿Qué ocurre si Azure SQL tarda en responder? | EF Core reintenta fallos transitorios, aumenta el tiempo de espera y un servicio realiza calentamiento al iniciar. |
| ¿Qué falta para el MVP? | Crear y confirmar pedidos, sus detalles, cálculo de totales, actualización transaccional de stock y autorización por roles. |

## Evidencias a mostrar

- `docs/story-map.md`: alcance, historias y criterios de aceptación.
- `docs/diagrama-entidad-relacion-final.svg`: modelo de datos.
- `Data/Migrations/`: evolución reproducible del esquema.
- CRUD de Productos y Clientes en ejecución.
- Imagen cargada desde la base de datos en el detalle del producto.

## Lista de verificación final

- [ ] La conexión apunta a la base de datos disponible para la clase.
- [ ] Las migraciones están aplicadas.
- [ ] Hay al menos una categoría y un producto para mostrar filtros y búsqueda.
- [ ] Hay un cliente de ejemplo para mostrar el CRUD.
- [ ] La presentación técnica y esta guía están abiertas antes de iniciar.
- [ ] No se muestran secretos, contraseñas ni datos personales reales.
