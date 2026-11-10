namespace Pedidos360.Web.Models;

public sealed class UsuarioRolViewModel
{
    public required string UsuarioId { get; init; }
    public required string Correo { get; init; }
    public required string RolActual { get; init; }
}
