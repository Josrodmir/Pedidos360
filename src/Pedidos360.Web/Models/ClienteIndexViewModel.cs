namespace Pedidos360.Web.Models;

public class ClienteIndexViewModel
{
    public required IReadOnlyList<Cliente> Clientes { get; init; }
    public string? Busqueda { get; init; }
    public int PaginaActual { get; init; }
    public int TotalPaginas { get; init; }
}
