using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class DispatchAdviceDetailConfiguration : IEntityTypeConfiguration<DispatchAdviceDetail>
    {
        public void Configure(EntityTypeBuilder<DispatchAdviceDetail> builder)
        {
            builder.ToTable("DispatchAdviceDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DispatchAdviceHeaderId)
                .HasColumnName("DispatchAdviceHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesOrderDetailId)
                .HasColumnName("SalesOrderDetailId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ItemId)
                .HasColumnName("ItemId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.LotId)
                .HasColumnName("LotId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.StartPackNo)
                .HasColumnName("StartPackNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.EndPackNo)
                .HasColumnName("EndPackNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DispatchQty)
                .HasColumnName("DispatchQty")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.PackTypeId)
                .HasColumnName("PackTypeId")
                .HasColumnType("int")
                .IsRequired();

            // Cross-module FKs (PackType, LotMaster) → ProductionManagement — no DB constraint

            builder.HasOne(t => t.SalesOrderDetail)
                .WithMany(d => d.DispatchAdviceDetails)
                .HasForeignKey(t => t.SalesOrderDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.DispatchAdviceHeaderId);
            builder.HasIndex(t => t.SalesOrderDetailId);
            builder.HasIndex(t => t.ItemId);
            builder.HasIndex(t => t.LotId);
            builder.HasIndex(t => t.PackTypeId);
        }
    }
}
