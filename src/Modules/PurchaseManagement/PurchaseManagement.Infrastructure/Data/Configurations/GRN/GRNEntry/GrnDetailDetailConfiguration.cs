using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PurchaseManagement.Infrastructure.Data.Configurations.GRN.GRNEntry
{
    public class GrnDetailDetailConfiguration : IEntityTypeConfiguration<GrnDetail>
    {
        public void Configure(EntityTypeBuilder<GrnDetail> builder)
        {
            builder.ToTable("GrnDetail", "Purchase");
            // Primary Key
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.GrnId)
                 .HasColumnName("GrnId")
                 .HasColumnType("int")
                 .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.GrnHeaderDetailsMaster)
                .WithMany(t => t.GrnDetails)
                .HasForeignKey(m => m.GrnId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); //


            builder.Property(m => m.PoId)
                 .HasColumnName("PoId")
                 .HasColumnType("int")
                 .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.GrnPoDetails)
                .WithMany(t => t.PoGrnDetails)
                .HasForeignKey(m => m.PoId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); //


            builder.Property(m => m.PoSlNoLocal)
                 .HasColumnName("PoSlNoLocal")
                 .HasColumnType("int");

            builder.Property(m => m.PoCategoryId)
                .HasColumnName("PoCategoryId")
                .HasColumnType("int")
                .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.PoGrnCategoryDetails)
                .WithMany(t => t.GrnDetailsPoCategory)
                .HasForeignKey(m => m.PoCategoryId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); //


            builder.Property(m => m.PoMethodId)
                 .HasColumnName("PoMethodId")
                 .HasColumnType("int")
                 .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.PoGrnMethodDetails)
                .WithMany(t => t.GrnDetailsPoMethod)
                .HasForeignKey(m => m.PoMethodId) // Foreign Key property in PartyContact
                .OnDelete(DeleteBehavior.Restrict); //


            builder.Property(m => m.ItemId)
                 .HasColumnName("ItemId")
                 .HasColumnType("int")
                 .IsRequired();

            builder.Property(dg => dg.OrderQuantity)
                .HasColumnType("decimal(18,3)")
                .IsRequired()
                .HasDefaultValue(0.000m); // Set default value to 0.00


            builder.Property(dg => dg.DcQuantity)
               .HasColumnType("decimal(18,3)")
               .IsRequired()
               .HasDefaultValue(0.000m); // Set default value to 0.00


            builder.Property(dg => dg.UpperTolerance)
               .HasColumnType("decimal(18,3)")
               .IsRequired(false)
               .HasDefaultValue(0.000m); // Set default value to 0.00

            builder.Property(dg => dg.LowerTolerance)
              .HasColumnType("decimal(18,3)")
              .IsRequired(false)
              .HasDefaultValue(0.000m);   // Set default value to 0.00


            builder.Property(dg => dg.ReceivedQuantity)
             .HasColumnType("decimal(18,3)")
             .IsRequired()
             .HasDefaultValue(0.000m); // Set default value to 0.00


            builder.Property(b => b.ExpiryDate)
                   .HasColumnName("ExpiryDate")
                   .IsRequired(false)
                   .HasColumnType("DatetimeOffset");

            builder.Property(m => m.BatchNumber)
                 .HasColumnName("BatchNumber")
                 .HasColumnType("nvarchar(250)");


            builder.Property(dg => dg.QcAcceptedQuantity)
              .HasColumnType("decimal(18,3)")
              .IsRequired(false)
              .HasDefaultValue(0.000m); // Set default value to 0.00



            builder.Property(dg => dg.QcRejectedQuantity)
              .HasColumnType("decimal(18,3)")
              .IsRequired(false)
              .HasDefaultValue(0.000m); // Set default value to 0.00

            builder.Property(m => m.QcRejectedRemarks)
                 .HasColumnName("QcRejectedRemarks")
                 .HasColumnType("nvarchar(250)");

            builder.Property(m => m.GrnDetailImage)
                 .HasColumnName("GrnDetailImage")
                 .HasColumnType("nvarchar(250)");

            // QC sign-off — moved from GrnHeader; QC is now per-line.
            // No DB FK on QcStatusId per design decision #6 — value-only int (matches prior pattern).
            builder.Property(b => b.QcRemarks)
                 .HasColumnName("QcRemarks")
                 .HasColumnType("varchar(250)")
                 .IsRequired(false);

            builder.Property(b => b.QcPersonName)
                 .HasColumnName("QcPersonName")
                 .HasColumnType("varchar(50)")
                 .IsRequired(false);

            builder.Property(m => m.QcStatusId)
                 .HasColumnName("QcStatusId")
                 .HasColumnType("int")
                 .IsRequired(false);

            builder.Property(b => b.QcDate)
                 .HasColumnName("QcDate")
                 .HasColumnType("datetimeoffset")
                 .IsRequired(false);

            builder.Property(b => b.QcApprovedIp)
                 .HasColumnName("QcApprovedIp")
                 .HasColumnType("varchar(20)")
                 .IsRequired(false);

            builder.Property(b => b.IsQcApproved)
                 .HasColumnName("IsQcApproved")
                 .HasColumnType("bit")
                 .HasDefaultValue(false)
                 .IsRequired();

            // QcStatusId is now a plain int? column (no MiscMaster nav, no DB FK).
        }

    }
}