using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.GRN.GRNEntry
{
    public class GrnHeaderDetailConfiguration : IEntityTypeConfiguration<GrnHeader>
    {
        public void Configure(EntityTypeBuilder<GrnHeader> builder)
        {
            builder.ToTable("GrnHeader", "Purchase");
            // Primary Key
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.GrnNo)
                   .HasColumnName("GrnNo")
                   .HasColumnType("nvarchar(100)")
                   .IsRequired();

            builder.Property(b => b.GrnDate)
                    .HasColumnName("GrnDate")
                    .IsRequired()
                    .HasColumnType("DatetimeOffset");

            builder.Property(m => m.UnitId)
                   .HasColumnName("UnitId")
                   .HasColumnType("int")
                   .IsRequired();

            // GateEntryId is now nullable and has no DB FK constraint. Legacy column once pointed
            // at Purchase.GateEntryHeader (deprecated); centralized flow stores Gate.GateInwardHdr.Id.
            builder.Property(m => m.GateEntryId)
                  .HasColumnName("GateEntryId")
                  .HasColumnType("int")
                  .IsRequired(false);

            // Ignore the legacy navigation so EF does not regenerate the FK.
            builder.Ignore(m => m.GrnHeaderDetails);

            builder.Property(m => m.PartyId)
                 .HasColumnName("PartyId")
                 .HasColumnType("int")
                 .IsRequired();

            builder.Property(m => m.InvoiceNo)
                 .HasColumnName("InvoiceNo")
                 .HasColumnType("nvarchar(100)")
                  .IsRequired();

            builder.Property(b => b.InvoiceDate)
                    .HasColumnName("InvoiceDate")
                    .IsRequired()
                    .HasColumnType("DatetimeOffset");

                        builder.Property(m => m.DcNo)
                             .HasColumnName("DcNo")
                             .HasColumnType("nvarchar(100)");

            builder.Property(b => b.DcDate)
                    .HasColumnName("DcDate")
                    .IsRequired(false)
                    .HasColumnType("DatetimeOffset");

            builder.Property(m => m.ReceivingWarehouseId)
                 .HasColumnName("ReceivingWarehouseId")
                 .HasColumnType("int")
                 .IsRequired();

            builder.Property(m => m.Remarks)
                 .HasColumnName("Remarks")
                 .HasColumnType("nvarchar(250)");

            builder.Property(t => t.IsGrnGenerated)
               .HasColumnName("IsGrnGenerated")
               .HasColumnType("bit")
               .IsRequired();

            builder.Property(m => m.GrnReceivedImage)
                 .HasColumnName("GrnReceivedImage")
                 .HasColumnType("nvarchar(250)");


            builder.Property(m => m.CreatedBy)
                 .HasColumnName("CreatedBy")
                 .HasColumnType("int")
                 .IsRequired();

            builder.Property(b => b.CreatedDate)
                    .HasColumnName("CreatedDate")
                    .IsRequired()
                    .HasColumnType("DatetimeOffset");

            builder.Property(b => b.CreatedByName)
                    .IsRequired()
                    .HasColumnType("varchar(50)");


            builder.Property(b => b.CreatedIP)
                    .IsRequired()
                    .HasColumnType("varchar(20)");

            builder.Property(m => m.ModifiedBy)
                 .HasColumnName("ModifiedBy")
                 .HasColumnType("int");

            builder.Property(b => b.ModifiedDate)
                    .HasColumnName("ModifiedDate")
                    .IsRequired(false)
                    .HasColumnType("DatetimeOffset");

            builder.Property(b => b.ModifiedByName)
                    .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                    .HasColumnType("varchar(20)");

            // QC sign-off fields (QcRemarks, QcPersonName, QcStatusId, QcDate, QcApprovedIp,
            // IsQcApproved) moved to GrnDetailConfiguration — QC is now per-line.
            // The FK to MiscMaster on QcStatusId is removed from the header side; if a per-line
            // FK is desired, it would be added to GrnDetailConfiguration (not done here per design
            // decision #6: no FK constraint, matches the prior pattern).

            builder.Property(m => m.QcWarehouseId)
                 .HasColumnName("QcWarehouseId")
                 .HasColumnType("int");

            builder.Property(m => m.RejectedImage)
              .HasColumnName("RejectedImage")
              .HasColumnType("nvarchar(250)");

         

        }
    }
}