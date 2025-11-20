using Microsoft.EntityFrameworkCore;
using ProductsService.Domain.Entities;

namespace ProductsService.Infrastructure.Persistence
{
    public class ProductDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Title);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Price);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CreatedAt);
        }
    }
}