using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder
{
    public class ServiceEntrySheetConfiguration : IEntityTypeConfiguration<ServiceEntrySheet>
    {


        public void Configure(EntityTypeBuilder<ServiceEntrySheet> b)
        {
        // Table configuration
            b.ToTable("ServiceEntrySheets", "Purchase");

            // Primary key configuration
            b.HasKey(x => x.Id);

            // Column configurations for string and nullable types
            b.Property(x => x.AttachmentFileName)
                .HasMaxLength(255)
                .IsRequired(false);

            b.Property(x => x.LineRemarks)
                .HasMaxLength(1000)
                .IsRequired(false);

            // Column configurations for decimal values (money/qty precision)
            b.Property(x => x.ActualQuantity)
                .HasColumnType("decimal(18,3)")  // Precision and scale
                .IsRequired(false); // Nullable

            b.Property(x => x.ActualRate)
                .HasColumnType("decimal(18,2)")  // Precision for rate values
                .IsRequired(false); // Nullable

            b.Property(x => x.ActualValue)
                .HasColumnType("decimal(18,2)")  // Precision for monetary values
                .IsRequired(false);

            b.Property(x => x.DiscountValue)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            b.Property(x => x.TaxValue)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            b.Property(x => x.TotalValue)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            // Column configurations for DateTime properties
            b.Property(x => x.SESDate)
                .HasColumnType("datetimeoffset")
                .IsRequired(true);

            b.Property(x => x.PODate)
                .HasColumnType("datetimeoffset")
                .IsRequired(true);

            b.Property(x => x.ValidityFrom)
                .HasColumnType("datetimeoffset")
                .IsRequired(false); // Nullable

            b.Property(x => x.ValidityTo)
                .HasColumnType("datetimeoffset")
                .IsRequired(false); // Nullable

            b.Property(x => x.WorkStartDate)
                .HasColumnType("datetimeoffset")
                .IsRequired(false); // Nullable

            b.Property(x => x.WorkEndDate)
                .HasColumnType("datetimeoffset")
                .IsRequired(false); // Nullable

            b.Property(x => x.StatusId)
                .IsRequired(true);  

          
           

            // Foreign key relationships configuration
            b.HasOne(x => x.PurchaseOrder)
                .WithMany(po => po.ServiceEntrySheets)
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete on PurchaseOrder deletion

            b.HasOne(x => x.ServiceCategory)
            .WithMany()
            .HasForeignKey(x => x.ServiceCategoryId)
            .OnDelete(DeleteBehavior.NoAction);  // No cascade delete

            b.HasOne(x => x.ContractType)
                .WithMany()
                .HasForeignKey(x => x.ContractTypeId)
                .OnDelete(DeleteBehavior.NoAction);  // No cascade delete

            b.HasOne(x => x.DiscountType)
                .WithMany()
                .HasForeignKey(x => x.DiscountTypeId)
                .OnDelete(DeleteBehavior.NoAction);  // No cascade delete

            b.HasOne(x => x.SESStatus)
                .WithMany()
                .HasForeignKey(x => x.SESStatusId)
                .OnDelete(DeleteBehavior.NoAction);  // Set to null if SESStatus is deleted

            // Relationship with ServiceEntryActivity (one-to-many)
            b.HasMany(x => x.Activities)
                .WithOne(a => a.EntrySheet)
                .HasForeignKey(a => a.EntrySheetId)
                .OnDelete(DeleteBehavior.Cascade);  // Cascade delete on SES deletion

            // Indexes to speed up querying by common fields
            b.HasIndex(x => x.PurchaseOrderId);
            b.HasIndex(x => x.ServiceCategoryId);
            b.HasIndex(x => x.ContractTypeId);
            b.HasIndex(x => x.SESStatusId);
         
        }
    }
}