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

            builder.Property(m => m.GateEntryId)
                  .HasColumnName("GateEntryId")
                  .HasColumnType("int")
                  .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.GrnHeaderDetails)
                .WithMany(t => t.GrnGateEntries)
                .HasForeignKey(m => m.GateEntryId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

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

            builder.Property(b => b.QcRemarks)
                    .HasColumnType("varchar(250)");

            builder.Property(b => b.QcPersonName)
                    .HasColumnType("varchar(50)");

            builder.Property(m => m.QcStatusId)
                 .HasColumnName("QcStatusId")
                 .HasColumnType("int")
                 .IsRequired(false);

            // Foreign Key Relationship
            builder.HasOne(m => m.GrnQcStatus)
                .WithMany(t => t.GrnQcStatusMisc)
                .HasForeignKey(m => m.QcStatusId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(b => b.QcDate)
                    .HasColumnName("QcDate")
                    .IsRequired(false)
                    .HasColumnType("DatetimeOffset");


            builder.Property(b => b.QcApprovedIp)
                    .HasColumnType("varchar(20)");

            builder.Property(t => t.IsQcApproved)
               .HasColumnName("IsQcApproved")
               .HasColumnType("bit")
               .IsRequired();

            builder.Property(m => m.QcWarehouseId)
                 .HasColumnName("QcWarehouseId")
                 .HasColumnType("int");

            builder.Property(m => m.RejectedImage)
              .HasColumnName("RejectedImage")
              .HasColumnType("nvarchar(250)");

         

        }
    }
}