using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesQuotationAmendmentDetailConfiguration : IEntityTypeConfiguration<SalesQuotationAmendmentDetail>
    {
        public void Configure(EntityTypeBuilder<SalesQuotationAmendmentDetail> builder)
        {
            builder.ToTable("SalesQuotationAmendmentDetail", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesQuotationAmendmentHeaderId)
                .HasColumnName("SalesQuotationAmendmentHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ChangeType)
                .HasColumnName("ChangeType")
                .HasColumnType("varchar(10)")
                .IsRequired();

            // Historical reference — NO DB FK constraint (quotation details can be physically deleted)
            builder.Property(t => t.SalesQuotationDetailId)
                .HasColumnName("SalesQuotationDetailId")
                .HasColumnType("int")
                .IsRequired();

            // Old Values (snapshot)
            builder.Property(t => t.OldItemId).HasColumnName("OldItemId").HasColumnType("int").IsRequired();
            builder.Property(t => t.OldQuantity).HasColumnName("OldQuantity").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.OldExMillRate).HasColumnName("OldExMillRate").HasColumnType("decimal(18,4)").IsRequired();
            builder.Property(t => t.OldDiscount).HasColumnName("OldDiscount").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.OldHSNId).HasColumnName("OldHSNId").HasColumnType("int").IsRequired();
            builder.Property(t => t.OldTaxPercentage).HasColumnName("OldTaxPercentage").HasColumnType("decimal(18,2)").IsRequired();

            // New Values (null for Removed)
            builder.Property(t => t.NewItemId).HasColumnName("NewItemId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.NewQuantity).HasColumnName("NewQuantity").HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(t => t.NewExMillRate).HasColumnName("NewExMillRate").HasColumnType("decimal(18,4)").IsRequired(false);
            builder.Property(t => t.NewDiscount).HasColumnName("NewDiscount").HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(t => t.NewHSNId).HasColumnName("NewHSNId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.NewTaxPercentage).HasColumnName("NewTaxPercentage").HasColumnType("decimal(18,2)").IsRequired(false);

            // Computed Fields
            builder.Property(t => t.NetRate).HasColumnName("NetRate").HasColumnType("decimal(18,4)").IsRequired();
            builder.Property(t => t.TotalAmount).HasColumnName("TotalAmount").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.TaxAmount).HasColumnName("TaxAmount").HasColumnType("decimal(18,2)").IsRequired();

            // Same-module FK constraint (to amendment header only)
            builder.HasOne(t => t.SalesQuotationAmendmentHeader)
                .WithMany(h => h.SalesQuotationAmendmentDetails)
                .HasForeignKey(t => t.SalesQuotationAmendmentHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // NO FK constraint on SalesQuotationDetailId — historical reference only

            // Indexes
            builder.HasIndex(t => t.SalesQuotationAmendmentHeaderId);
            builder.HasIndex(t => t.SalesQuotationDetailId);
        }
    }
}
