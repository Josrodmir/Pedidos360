using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Web.Models;

namespace Pedidos360.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoDetalle> PedidoDetalles => Set<PedidoDetalle>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // SQL Server uses varbinary(max); Aiven MySQL requires a LONGBLOB column.
        // Fluent configuration overrides the data annotation in Producto.
        var imageColumnType = Database.ProviderName?.Contains("MySql", StringComparison.OrdinalIgnoreCase) == true
            ? "longblob"
            : "varbinary(max)";
        builder.Entity<Producto>().Property(p => p.ImagenBytes).HasColumnType(imageColumnType);

        builder.Entity<Categoria>().HasIndex(c => c.Nombre).IsUnique();
        builder.Entity<Cliente>().HasIndex(c => c.Cedula).IsUnique();
        builder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Pedido>().HasOne(p => p.Cliente).WithMany().HasForeignKey(p => p.ClienteId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<PedidoDetalle>().HasOne(d => d.Pedido).WithMany(p => p.Detalles).HasForeignKey(d => d.PedidoId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<PedidoDetalle>().HasOne(d => d.Producto).WithMany().HasForeignKey(d => d.ProductoId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Tecnología" },
            new Categoria { Id = 2, Nombre = "Oficina" },
            new Categoria { Id = 3, Nombre = "Limpieza" });
    }
}
