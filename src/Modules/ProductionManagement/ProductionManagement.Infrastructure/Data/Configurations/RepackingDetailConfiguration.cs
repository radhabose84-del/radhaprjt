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

            builder.Property(t => t.StartPackNo)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.EndPackNo)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldStartPackNo)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldEndPackNo)
                .HasColumnType("int")
                .IsRequired();

            // Indexes
            builder.HasIndex(t => t.RepackHeaderId);

            // FK to header is configured in RepackingHeaderConfiguration via HasMany
            // Cross-module FKs (OldWarehouseId, OldBinId) — NO DB constraint
        }
    }
}
