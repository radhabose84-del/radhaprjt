using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.Infrastructure.Data.Configurations
{
    public class RepackingDetailConfiguration : IEntityTypeConfiguration<RepackingDetail>
    {
        public void Configure(EntityTypeBuilder<RepackingDetail> builder)
        {
            builder.ToTable("RepackingDetail", "Production");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.RepackHeaderId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldStartPackNo)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldEndPackNo)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldNetWeightPerPack)
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.OldTotalBags)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldNetWeight)
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.OldWarehouseId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldBinId)
                .HasColumnType("int")
                .IsRequired();

            // Indexes
            builder.HasIndex(t => t.RepackHeaderId);
            builder.HasIndex(t => t.OldWarehouseId);
            builder.HasIndex(t => t.OldBinId);

            // FK to header is configured in RepackingHeaderConfiguration via HasMany
            // Cross-module FKs (OldWarehouseId, OldBinId) — NO DB constraint
        }
    }
}
