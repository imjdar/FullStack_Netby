using Inventory.Products.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Products.Api.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        // El constructor le pasa la configuración (la cadena de conexión) a la base
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Tabla Productos
        public DbSet<Product> Productos => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Se configura precision del precio
            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
        }
    }
}