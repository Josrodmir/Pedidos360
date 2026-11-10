namespace Pedidos360.Web.Security;

/// <summary>Business roles used by authorization rules throughout Pedidos360.</summary>
public static class AppRoles
{
    public const string Administrador = "Administrador";
    public const string Ventas = "Ventas";
    public const string Contabilidad = "Contabilidad";
    public const string Inventario = "Inventario";

    public static IReadOnlyList<string> Lista { get; } = [Administrador, Ventas, Contabilidad, Inventario];

    public const string CatalogoLectura = Administrador + "," + Ventas + "," + Contabilidad + "," + Inventario;
    public const string CatalogoGestion = Administrador + "," + Inventario;
    public const string ClientesGestion = Administrador + "," + Ventas;
    public const string PedidosLectura = Administrador + "," + Ventas + "," + Contabilidad + "," + Inventario;
    public const string PedidosGestion = Administrador + "," + Ventas;
}
