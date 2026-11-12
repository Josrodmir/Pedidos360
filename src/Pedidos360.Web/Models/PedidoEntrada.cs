using System.ComponentModel.DataAnnotations;

namespace Pedidos360.Web.Models;

public record LineaPedidoEntrada(int ProductoId, [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")] int Cantidad, [Range(0, 100)] decimal Descuento);
public record PedidoEntrada(int ClienteId, List<LineaPedidoEntrada> Lineas);
