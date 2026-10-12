namespace Pedidos360.Web.Models;

public class ProductoIndexViewModel
{
    public required IReadOnlyList<Producto> Productos { get; init; }
    public required IReadOnlyList<Categoria> Categorias { get; init; }
    public string? Busqueda { get; init; }
    public int? CategoriaId { get; init; }
    public int PaginaActual { get; init; }
    public int TotalPaginas { get; init; }
}
