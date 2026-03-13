using Microsoft.EntityFrameworkCore;
using Inventory.Transactions.Api.Domain.Entities;

namespace Inventory.Transactions.Api.Infra.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>().Property(t => t.UnitPrice).HasPrecision(18, 2);
            modelBuilder.Entity<Transaction>().Property(t => t.TotalPrice).HasPrecision(18, 2);
        }
    }
}
