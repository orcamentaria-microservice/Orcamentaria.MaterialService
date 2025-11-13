using Microsoft.EntityFrameworkCore;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Infrastructure.Configurations;

namespace Orcamentaria.MaterialService.Infrastructure.Contexts
{
    public class MySqlContext : DbContext
    {
        public MySqlContext(DbContextOptions<MySqlContext> options)
        : base(options)
        {
        }

        public DbSet<Material> Materials { get; set; }
        public DbSet<MaterialType> MaterialTypes { get; set; }
        public DbSet<MaterialSupplier> MaterialSuppliers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MaterialConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialSupplierConfiguration());
        }
    }
}
