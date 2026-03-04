using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orcamentaria.MaterialService.Domain.Models;

namespace Orcamentaria.MaterialService.Infrastructure.Configurations
{
    public class MaterialSupplierConfiguration : IEntityTypeConfiguration<MaterialSupplier>
    {
        public void Configure(EntityTypeBuilder<MaterialSupplier> builder)
        {
            builder.ToTable("T_MATERIAL_SUPPLIER");
            builder.HasKey(x => x.Id);
            builder.Property(p => p.Id)
                .HasColumnName("ID")
                .HasColumnType("BIGINT")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.MaterialId)
                .HasColumnName("MATERIAL_ID")
                .HasColumnType("BIGINT")
                .IsRequired();

            builder.Property(p => p.SupplierId)
                .HasColumnName("SUPPLIER_ID")
                .HasColumnType("BIGINT")
                .IsRequired();

            builder.Property(p => p.CompanyId)
                .HasColumnName("COMPANY_ID")
                .HasColumnType("BIGINT")
                .IsRequired();
        }
    }
}
