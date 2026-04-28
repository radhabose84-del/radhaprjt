using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesQuotationAmendmentHeaderConfiguration : IEntityTypeConfiguration<SalesQuotationAmendmentHeader>
    {
        public void Configure(EntityTypeBuilder<SalesQuotationAmendmentHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("SalesQuotationAmendmentHeader", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesQuotationHeaderId)
                .HasColumnName("SalesQuotationHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AmendmentNo)
                .HasColumnName("AmendmentNo")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(t => t.RevisionNumber)
                .HasColumnName("RevisionNumber")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.AmendmentDate)
                .HasColumnName("AmendmentDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.Reason)
                .HasColumnName("Reason")
                .HasColumnType("nvarchar(500)")
                .IsRequired();

            // Header-level Summary Fields
            builder.Property(t => t.FreightCharges).HasColumnName("FreightCharges").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.OtherCharges).HasColumnName("OtherCharges").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.TotalBasicAmount).HasColumnName("TotalBasicAmount").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.TotalDiscount).HasColumnName("TotalDiscount").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.NetTaxableAmount).HasColumnName("NetTaxableAmount").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.TotalTax).HasColumnName("TotalTax").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.GrandTotal).HasColumnName("GrandTotal").HasColumnType("decimal(18,2)").IsRequired();

            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ApprovedBy)
                .HasColumnName("ApprovedBy")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ApprovedDate)
                .HasColumnName("ApprovedDate")
                .IsRequired(false);

            // Status & Audit
            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Same-module FK constraints
            builder.HasOne(t => t.SalesQuotationHeader)
                .WithMany(h => h.SalesQuotationAmendmentHeaders)
                .HasForeignKey(t => t.SalesQuotationHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.StatusMisc)
                .WithMany(m => m.SalesQuotationAmendmentHeadersAsStatus)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Child collection — reverse navigation (Header → Details)
            builder.HasMany(t => t.SalesQuotationAmendmentDetails)
                .WithOne(d => d.SalesQuotationAmendmentHeader)
                .HasForeignKey(d => d.SalesQuotationAmendmentHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.AmendmentNo).IsUnique();
            builder.HasIndex(t => t.SalesQuotationHeaderId);
            builder.HasIndex(t => t.UnitId);
            builder.HasIndex(t => t.StatusId);
        }
    }
}
