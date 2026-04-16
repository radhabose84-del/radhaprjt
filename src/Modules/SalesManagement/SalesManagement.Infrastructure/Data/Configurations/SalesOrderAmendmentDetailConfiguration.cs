using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesOrderAmendmentDetailConfiguration : IEntityTypeConfiguration<SalesOrderAmendmentDetail>
    {
        public void Configure(EntityTypeBuilder<SalesOrderAmendmentDetail> builder)
        {
            builder.ToTable("SalesOrderAmendmentDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesOrderAmendmentHeaderId)
                .HasColumnName("SalesOrderAmendmentHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ChangeType)
                .HasColumnName("ChangeType")
                .HasColumnType("varchar(10)")
                .IsRequired();

            builder.Property(t => t.SalesOrderDetailId)
                .HasColumnName("SalesOrderDetailId")
                .HasColumnType("int")
                .IsRequired();

            // Old Values (snapshot)
            builder.Property(t => t.OldItemId)
                .HasColumnName("OldItemId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldQtyInBags)
                .HasColumnName("OldQtyInBags")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.OldExMillRate)
                .HasColumnName("OldExMillRate")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.OldExpectedDeliveryDate)
                .HasColumnName("OldExpectedDeliveryDate")
                .HasColumnType("date")
                .IsRequired();

            // New Values (null for Removed)
            builder.Property(t => t.NewQtyInBags)
                .HasColumnName("NewQtyInBags")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.NewExMillRate)
                .HasColumnName("NewExMillRate")
                .HasColumnType("decimal(18,3)")
                .IsRequired(false);

            builder.Property(t => t.NewExpectedDeliveryDate)
                .HasColumnName("NewExpectedDeliveryDate")
                .HasColumnType("date")
                .IsRequired(false);

            // Detail-level Computed Fields
            builder.Property(t => t.TotalWeight).HasColumnName("TotalWeight").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.DiscountPerUnit).HasColumnName("DiscountPerUnit").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.TaxableAmount).HasColumnName("TaxableAmount").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.TaxAmount).HasColumnName("TaxAmount").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.TCSAmount).HasColumnName("TCSAmount").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.NetAmount).HasColumnName("NetAmount").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.NetRatePerKg).HasColumnName("NetRatePerKg").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.PendingQty).HasColumnName("PendingQty").HasColumnType("int").IsRequired();
            builder.Property(t => t.AgentCommissionPercentage).HasColumnName("AgentCommissionPercentage").HasColumnType("decimal(18,3)").IsRequired(false);

            // Same-module FK constraints
            builder.HasOne(t => t.SalesOrderAmendmentHeader)
                .WithMany(h => h.SalesOrderAmendmentDetails)
                .HasForeignKey(t => t.SalesOrderAmendmentHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.SalesOrderDetail)
                .WithMany()
                .HasForeignKey(t => t.SalesOrderDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.SalesOrderAmendmentHeaderId);
            builder.HasIndex(t => t.SalesOrderDetailId);
        }
    }
}
