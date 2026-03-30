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

            builder.Property(t => t.RepackingHeaderId).HasColumnType("int").IsRequired();
            builder.Property(t => t.ItemId).HasColumnType("int").IsRequired();
            builder.Property(t => t.LotId).HasColumnType("int").IsRequired();
            builder.Property(t => t.BinId).HasColumnType("int").IsRequired();
            builder.Property(t => t.WarehouseId).HasColumnType("int").IsRequired();
            builder.Property(t => t.PackTypeId).HasColumnType("int").IsRequired();
            builder.Property(t => t.StartPackNo).HasColumnType("int").IsRequired();
            builder.Property(t => t.EndPackNo).HasColumnType("int").IsRequired();
            builder.Property(t => t.NetWeightPerPack).HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.TotalBags).HasColumnType("int").IsRequired();
            builder.Property(t => t.NetWeight).HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.OldPackDetailId).HasColumnType("int").IsRequired();

            builder.HasIndex(t => t.RepackingHeaderId);
            builder.HasIndex(t => t.OldPackDetailId);
            builder.HasIndex(t => t.LotId);

            // Same-module FK: LotId → LotMaster
            builder.HasOne(t => t.LotMaster)
                .WithMany()
                .HasForeignKey(t => t.LotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: PackTypeId → PackType (target pack type)
            builder.HasOne(t => t.PackType)
                .WithMany()
                .HasForeignKey(t => t.PackTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: OldPackDetailId → ProductionPackDetail (source detail)
            builder.HasOne(t => t.OldPackDetail)
                .WithMany()
                .HasForeignKey(t => t.OldPackDetailId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
