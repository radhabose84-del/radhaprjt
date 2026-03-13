using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class EInvoiceHeaderConfiguration : IEntityTypeConfiguration<EInvoiceHeader>
    {
        public void Configure(EntityTypeBuilder<EInvoiceHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("EInvoiceHeader", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.UnitId).HasColumnName("UnitId").HasColumnType("int").IsRequired();
            builder.Property(t => t.InvoiceNo).HasColumnName("InvoiceNo").HasColumnType("varchar(30)").IsRequired(false);
            builder.Property(t => t.InvoiceDate).HasColumnName("InvoiceDate").HasColumnType("date").IsRequired();
            builder.Property(t => t.IrnNumber).HasColumnName("IrnNumber").HasColumnType("varchar(100)").IsRequired(false);
            builder.Property(t => t.AckNo).HasColumnName("AckNo").HasColumnType("varchar(50)").IsRequired(false);
            builder.Property(t => t.AckDate).HasColumnName("AckDate").IsRequired(false);
            builder.Property(t => t.CGST).HasColumnName("CGST").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.SGST).HasColumnName("SGST").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.IGST).HasColumnName("IGST").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TCS).HasColumnName("TCS").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.RoundOff).HasColumnName("RoundOff").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.InvoiceAmount).HasColumnName("InvoiceAmount").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.PartyId).HasColumnName("PartyId").HasColumnType("int").IsRequired();
            builder.Property(t => t.GstNo).HasColumnName("GstNo").HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(t => t.Discount).HasColumnName("Discount").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.Cess).HasColumnName("Cess").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.OtherCharges).HasColumnName("OtherCharges").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.ReverseCharge).HasColumnName("ReverseCharge").HasColumnType("bit").HasDefaultValue(false).IsRequired();
            builder.Property(t => t.SignInvoice).HasColumnName("SignInvoice").HasColumnType("nvarchar(max)").IsRequired(false);
            builder.Property(t => t.SignQrCode).HasColumnName("SignQrCode").HasColumnType("nvarchar(max)").IsRequired(false);
            builder.Property(t => t.EwbNo).HasColumnName("EwbNo").HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(t => t.EwbDate).HasColumnName("EwbDate").IsRequired(false);
            builder.Property(t => t.EwbValidTill).HasColumnName("EwbValidTill").IsRequired(false);
            builder.Property(t => t.StatusId).HasColumnName("StatusId").HasColumnType("int").IsRequired(false);

            builder.Property(b => b.IsActive).HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            builder.Property(b => b.IsDeleted).HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Child collection (same-module FK to EInvoiceDetail)
            builder.HasMany(t => t.EInvoiceDetails)
                .WithOne(d => d.EInvoiceHeader)
                .HasForeignKey(d => d.EInvoiceHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => t.InvoiceNo);
            builder.HasIndex(t => t.IrnNumber).IsUnique();
            builder.HasIndex(t => t.PartyId);
            builder.HasIndex(t => t.UnitId);
            builder.HasIndex(t => t.StatusId);
        }
    }
}
