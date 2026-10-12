# Modelo entidad-relación y normalización

```mermaid
erDiagram
    CATEGORIA ||--o{ PRODUCTO : clasifica
    CLIENTE ||--o{ PEDIDO : realiza
    PEDIDO ||--|{ PEDIDO_DETALLE : contiene
    PRODUCTO ||--o{ PEDIDO_DETALLE : solicitado_en
    USUARIO ||--o{ PEDIDO : registra

    CATEGORIA { int Id PK string Nombre }
    PRODUCTO { int Id PK int CategoriaId FK string Nombre decimal Precio decimal ImpuestoPorc int Stock string ImagenUrl bool Activo }
    CLIENTE { int Id PK string Nombre string Cedula UK string Correo string Telefono string Direccion }
    PEDIDO { int Id PK int ClienteId FK string UsuarioId FK datetime Fecha decimal Subtotal decimal Impuestos decimal Total string Estado }
    PEDIDO_DETALLE { int Id PK int PedidoId FK int ProductoId FK int Cantidad decimal PrecioUnit decimal Descuento decimal ImpuestoPorc decimal TotalLinea }
    USUARIO { string Id PK string Email }
```

## Decisiones de diseño

- **Primera forma normal:** todos los atributos son atómicos; los detalles del pedido se separan de su encabezado.
- **Segunda forma normal:** los datos del producto y del cliente no se repiten por línea de pedido. Cada detalle depende de su identificador.
- **Tercera forma normal:** la categoría se almacena una vez y se referencia por clave foránea; se evita repetir su nombre en Producto. La cédula del cliente es única.
- Los importes de `Pedido` y `PedidoDetalle` se persistirán en el segundo hito para auditoría, aunque se calculen a partir de las líneas.

## Integridad

- Producto requiere categoría existente y no permite stock negativo.
- Cliente impide identificaciones duplicadas.
- La relación Producto-Categoría restringe eliminar una categoría que tenga productos.
