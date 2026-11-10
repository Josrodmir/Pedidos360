using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedidos360.Web.Models;

public class Pedido
{
    public int Id { get; set; }
    [Display(Name = "Cliente")] public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public string? UsuarioId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "decimal(12,2)")] public decimal Subtotal { get; set; }
    [Column(TypeName = "decimal(12,2)")] public decimal Impuestos { get; set; }
    [Column(TypeName = "decimal(12,2)")] public decimal Total { get; set; }
    [StringLength(30)] public string Estado { get; set; } = "Confirmado";
    public ICollection<PedidoDetalle> Detalles { get; set; } = [];
}
