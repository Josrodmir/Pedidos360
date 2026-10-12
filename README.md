# Pedidos360

Sistema web B2B para la gestión de inventario, clientes y pedidos. Desarrollado como proyecto académico de Programación Avanzada.

## Tecnologías

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core Code First y Migrations
- SQL Server / Azure SQL Database
- ASP.NET Core Identity
- Bootstrap 5

## Estado del proyecto

Primer avance: CRUD de Productos y Clientes, validaciones cliente/servidor, filtros, paginación, modelo inicial EF Core y prototipo de interfaz. Los pedidos, roles y AJAX corresponden al siguiente hito.

## Ejecutar localmente

1. Abra `Pedidos360.slnx` con Visual Studio.
2. Configure la cadena `DefaultConnection` en User Secrets. Para Azure SQL use el formato siguiente, reemplazando solo `<PASSWORD>` localmente:

   ```text
   Server=tcp:pedidos360-josrodmir.database.windows.net,1433;Initial Catalog=Pedidos360DB;User ID=pedidos360admin@pedidos360-josrodmir;Password=<PASSWORD>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   ```

3. Ejecute `Update-Database` desde la Consola del Administrador de Paquetes.
4. Presione F5 para iniciar la aplicación.

> Las credenciales de Azure SQL nunca se guardan en el repositorio.

## Documentación del avance

- [Modelo ER y normalización](docs/modelo-er.md)
- [Story map y criterios de aceptación](docs/story-map.md)
