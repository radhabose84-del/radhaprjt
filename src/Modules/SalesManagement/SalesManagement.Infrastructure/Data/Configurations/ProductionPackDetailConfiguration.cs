using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class ProductionPackDetailConfiguration : IEntityTypeConfiguration<ProductionPackDetail>
    {
        public void Configure(EntityTypeBuilder<ProductionPackDetail> builder)
        {
            builder.ToTable("ProductionPackDetail", "Production");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ProductionPackHeaderId)
                .HasColumnName("ProductionPackHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ItemSno)
                .HasColumnName("ItemSno")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.LotId)
                .HasColumnName("LotId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ItemId)
                .HasColumnName("ItemId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PackTypeId)
                .HasColumnName("PackTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.NetWeightPerPack)
                .HasColumnName("NetWeightPerPack")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.StartPackNo)
                .HasColumnName("StartPackNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.EndPackNo)
                .HasColumnName("EndPackNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TotalBags)
                .HasColumnName("TotalBags")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TotalNetWeight)
                .HasColumnName("TotalNetWeight")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.BinId)
                .HasColumnName("BinId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.QualityStatusId)
                .HasColumnName("QualityStatusId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.LineRemarks)
                .HasColumnName("LineRemarks")
                .HasColumnType("nvarchar(250)")
                .IsRequired(false);

            // Same-module FK constraints
            builder.HasOne(t => t.LotMaster)
                .WithMany(l => l.ProductionPackDetails)
                .HasForeignKey(t => t.LotId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.PackType)
                .WithMany(p => p.ProductionPackDetails)
                .HasForeignKey(t => t.PackTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.QualityStatusMisc)
                .WithMany(m => m.ProductionPackDetailsAsQualityStatus)
                .HasForeignKey(t => t.QualityStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.ProductionPackHeaderId);
            builder.HasIndex(t => t.LotId);
            builder.HasIndex(t => t.PackTypeId);
            builder.HasIndex(t => t.BinId);
        }
    }
}
