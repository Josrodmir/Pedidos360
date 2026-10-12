using System.ComponentModel.DataAnnotations;

namespace Pedidos360.Web.Models;

public class Cliente
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120)] public string Nombre { get; set; } = string.Empty;
    [Required(ErrorMessage = "La cédula o identificación jurídica es obligatoria.")]
    [StringLength(30)] [Display(Name = "Cédula / Jurídica")] public string Cedula { get; set; } = string.Empty;
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [StringLength(150)] public string Correo { get; set; } = string.Empty;
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(30)] public string Telefono { get; set; } = string.Empty;
    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(250)] public string Direccion { get; set; } = string.Empty;
}
