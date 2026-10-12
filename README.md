# Pedidos360

Sistema web B2B para la gestión de inventario, clientes y pedidos. Desarrollado como proyecto académico de Programación Avanzada.

## Tecnologías

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core Code First y Migrations
- SQL Server / Azure SQL Database
- ASP.NET Core Identity
- Bootstrap 5

## Estado del proyecto

Primer avance en desarrollo: solución MVC base, autenticación Identity, modelo inicial de categorías, productos y clientes, y migración inicial de base de datos.

## Ejecutar localmente

1. Abra `Pedidos360.slnx` con Visual Studio.
2. Configure la cadena `DefaultConnection` en User Secrets para SQL Server o Azure SQL.
3. Ejecute `Update-Database` desde la Consola del Administrador de Paquetes.
4. Presione F5 para iniciar la aplicación.

> Las credenciales de Azure SQL nunca se guardan en el repositorio.
