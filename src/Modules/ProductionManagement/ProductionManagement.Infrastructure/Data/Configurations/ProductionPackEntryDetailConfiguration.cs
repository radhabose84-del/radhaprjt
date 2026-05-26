using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.Infrastructure.Data.Configurations
{
    public class ProductionPackEntryDetailConfiguration : IEntityTypeConfiguration<ProductionPackEntryDetail>
    {
        public void Configure(EntityTypeBuilder<ProductionPackEntryDetail> builder)
        {
            builder.ToTable("ProductionPackEntryDetail", "Production");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.ProductionPackEntryId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.LotId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PackTypeId)
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.NetWeightPerPack)
                .HasColumnType("decimal(18,3)")
                .IsRequired(false);

            builder.Property(t => t.StartPackNo)
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.EndPackNo)
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.OpeningLooseKgs)
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TotalProductionKgs)
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TotalBags)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TotalNetWeight)
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.ProductionKgs)
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.LooseConeKgs)
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TypeId)
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.Remarks)
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            // Indexes
            builder.HasIndex(t => t.ProductionPackEntryId);
            builder.HasIndex(t => t.LotId);

            // FK to header is configured in ProductionPackEntryConfiguration via HasMany
        }
    }
}
