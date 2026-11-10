using System.ComponentModel.DataAnnotations;

namespace Pedidos360.Web.Models;

public record LineaPedidoEntrada(int ProductoId, [Range(1, 9999)] int Cantidad, [Range(0, 100)] decimal Descuento);
public record PedidoEntrada(int ClienteId, List<LineaPedidoEntrada> Lineas);
