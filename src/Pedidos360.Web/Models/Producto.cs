using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedidos360.Web.Models;

public class Producto
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120)] public string Nombre { get; set; } = string.Empty;
    [Required(ErrorMessage = "Seleccione una categoría.")]
    [Display(Name = "Categoría")] public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    [Range(0.01, 99999999, ErrorMessage = "El precio debe ser mayor que cero.")]
    [Column(TypeName = "decimal(12,2)")] public decimal Precio { get; set; }
    [Range(0, 100, ErrorMessage = "El impuesto debe estar entre 0 y 100.")]
    [Display(Name = "Impuesto (%)")]
    [Column(TypeName = "decimal(5,2)")] public decimal ImpuestoPorc { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }
    // Maintains the previous URL values during the migration. New images are stored as bytes.
    [Column("ImagenUrl")]
    public string? ImagenUrlLegado { get; set; }

    [Column(TypeName = "varbinary(max)")]
    public byte[]? ImagenBytes { get; set; }

    [StringLength(100)]
    public string? ImagenTipo { get; set; }

    [NotMapped]
    [Display(Name = "Imagen del producto")]
    public IFormFile? ImagenArchivo { get; set; }
    public bool Activo { get; set; } = true;
}
