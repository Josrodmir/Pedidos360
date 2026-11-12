using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedidos360.Web.Models;

public class PedidoDetalle
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido? Pedido { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")] public int Cantidad { get; set; }
    [Column(TypeName = "decimal(12,2)")] public decimal PrecioUnitario { get; set; }
    [Range(0, 100)] [Column(TypeName = "decimal(5,2)")] public decimal Descuento { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal ImpuestoPorc { get; set; }
    [Column(TypeName = "decimal(12,2)")] public decimal TotalLinea { get; set; }
}
