# Pedidos360

Sistema web B2B para la gestión de inventario, clientes y pedidos. Desarrollado como proyecto académico de Programación Avanzada.

## Tecnologías

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core Code First y Migrations
- SQL Server / Azure SQL Database
- ASP.NET Core Identity
- Bootstrap 5

## Estado del proyecto

Segundo avance funcional: CRUD de productos y clientes, validaciones cliente/servidor, filtros, paginación, almacenamiento de imágenes en la base de datos, ASP.NET Core Identity con roles, pedidos con actualización de inventario y API interna consumida con AJAX para buscar productos y calcular totales sin recargar la página.

## Ejecutar localmente

1. Abra `Pedidos360.slnx` con Visual Studio.
2. Configure la cadena `DefaultConnection` en User Secrets. Para Azure SQL use el formato siguiente, reemplazando solo `<PASSWORD>` localmente:

   ```text
   Server=tcp:pedidos360-josrodmir.database.windows.net,1433;Initial Catalog=Pedidos360DB;User ID=pedidos360admin@pedidos360-josrodmir;Password=<PASSWORD>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   ```

3. Ejecute `Update-Database` desde la Consola del Administrador de Paquetes. Esto aplica también la migración de `Pedidos` y `PedidoDetalles`.
4. Presione F5 para iniciar la aplicación.

> Las credenciales de Azure SQL nunca se guardan en el repositorio.

## Despliegue en Render

El repositorio incluye un `Dockerfile` para desplegar como **Web Service / Docker**. En Render configure la variable de entorno `ConnectionStrings__DefaultConnection` con la cadena de Azure SQL. No incluya la contraseña en GitHub ni en `appsettings.json`.

### Roles y control de acceso

ASP.NET Identity almacena las contraseñas como hashes, nunca como texto plano. Los perfiles de acceso son: **Administrador**, **Ventas**, **Contabilidad** e **Inventario**. Administración puede asignarlos desde `Usuarios`; Inventario gestiona catálogo, Ventas gestiona clientes y pedidos, y Contabilidad consulta pedidos.

Al crear el primer usuario, asígnele el rol `Administrador` desde Azure SQL una sola vez; luego ese usuario administrará los demás perfiles desde la pantalla `Usuarios`.

## Documentación del avance

- [Diagrama entidad-relación final](docs/diagrama-entidad-relacion-final.svg)
- [Story map y criterios de aceptación](docs/story-map.md)
- [Guía para el caso de estudio](docs/guia-caso-estudio.md)
- [Checklist de pruebas del hito 2](docs/checklist-hito-2.md)
