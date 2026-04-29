using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class SalesQuotationHeaderConfiguration : IEntityTypeConfiguration<SalesQuotationHeader>
    {
        public void Configure(EntityTypeBuilder<SalesQuotationHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("SalesQuotationHeader", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.QuotationNo)
                .HasColumnName("QuotationNo")
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            builder.Property(t => t.CustomerId)
                .HasColumnName("CustomerId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.QuotationDate)
                .HasColumnName("QuotationDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.SalesEnquiryId)
                .HasColumnName("SalesEnquiryId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ContactPersonId)
                .HasColumnName("ContactPersonId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ValidityDate)
                .HasColumnName("ValidityDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.PaymentTermId)
                .HasColumnName("PaymentTermId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("varchar(500)")
                .IsRequired(false);

            builder.Property(t => t.DeliveryTermId)
                .HasColumnName("DeliveryTermId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.FreightCharges)
                .HasColumnName("FreightCharges")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.OtherCharges)
                .HasColumnName("OtherCharges")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.TotalBasicAmount)
                .HasColumnName("TotalBasicAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.TotalDiscount)
                .HasColumnName("TotalDiscount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.NetTaxableAmount)
                .HasColumnName("NetTaxableAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.TotalTax)
                .HasColumnName("TotalTax")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.GrandTotal)
                .HasColumnName("GrandTotal")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.RevisionNumber)
                .HasColumnName("RevisionNumber")
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

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

            // Audit fields
            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Same-module FK constraints
            builder.HasOne(t => t.SalesEnquiryHeader)
                .WithMany()
                .HasForeignKey(t => t.SalesEnquiryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ContactPerson)
                .WithMany()
                .HasForeignKey(t => t.ContactPersonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.DeliveryTerm)
                .WithMany()
                .HasForeignKey(t => t.DeliveryTermId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.StatusMisc)
                .WithMany(m => m.SalesQuotationHeadersAsStatus)
                .HasForeignKey(t => t.StatusId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.QuotationNo)
                .IsUnique()
                .HasFilter("[QuotationNo] IS NOT NULL");
            builder.HasIndex(t => t.CustomerId);
            builder.HasIndex(t => t.SalesEnquiryId);
            builder.HasIndex(t => t.PaymentTermId);
            builder.HasIndex(t => t.DeliveryTermId);
        }
    }
}
