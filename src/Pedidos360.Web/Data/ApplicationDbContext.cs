using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pedidos360.Web.Models;

namespace Pedidos360.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Categoria>().HasIndex(c => c.Nombre).IsUnique();
        builder.Entity<Cliente>().HasIndex(c => c.Cedula).IsUnique();
        builder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Tecnología" },
            new Categoria { Id = 2, Nombre = "Oficina" },
            new Categoria { Id = 3, Nombre = "Limpieza" });
    }
}
