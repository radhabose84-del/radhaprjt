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

            builder.Property(t => t.Id).HasColumnType("int").IsRequired();
            builder.Property(t => t.UnitId).HasColumnType("int").IsRequired();
            builder.Property(t => t.DocType).HasColumnType("varchar(5)").IsRequired(false);
            builder.Property(t => t.SupplyType).HasColumnType("varchar(10)").IsRequired(false);
            builder.Property(t => t.InvoiceNo).HasColumnType("varchar(30)").IsRequired(false);
            builder.Property(t => t.InvoiceDate).HasColumnType("date").IsRequired();
            builder.Property(t => t.PlaceOfSupply).HasColumnType("varchar(2)").IsRequired(false);
            builder.Property(t => t.IrnNumber).HasColumnType("varchar(100)").IsRequired(false);
            builder.Property(t => t.AckNo).HasColumnType("varchar(50)").IsRequired(false);
            builder.Property(t => t.AckDate).IsRequired(false);
            builder.Property(t => t.SignInvoice).HasColumnType("nvarchar(max)").IsRequired(false);
            builder.Property(t => t.SignQrCode).HasColumnType("nvarchar(max)").IsRequired(false);
            builder.Property(t => t.IrnStatus).HasColumnType("varchar(20)").HasDefaultValue("Pending").IsRequired(false);
            builder.Property(t => t.ErrorCode).HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(t => t.ErrorMessage).HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(t => t.PartyId).HasColumnType("int").IsRequired();
            builder.Property(t => t.GstNo).HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(t => t.ReverseCharge).HasColumnType("bit").HasDefaultValue(false).IsRequired();
            builder.Property(t => t.CGST).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.SGST).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.IGST).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.Cess).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.StateCess).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TCS).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.Discount).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.OtherCharges).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.RoundOff).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.InvoiceAmount).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.Remarks).HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(t => t.StatusId).HasColumnType("int").IsRequired(false);
            builder.Property(t => t.EWaybillCreated).HasColumnType("bit").HasDefaultValue(false).IsRequired();

            builder.Property(b => b.IsActive).HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            builder.Property(b => b.IsDeleted).HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
            builder.Property(t => t.CreatedBy).HasColumnType("int");
            builder.Property(t => t.CreatedDate);
            builder.Property(t => t.CreatedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnType("int");
            builder.Property(t => t.ModifiedDate);
            builder.Property(t => t.ModifiedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnType("varchar(50)");

            // Same-module FK → EInvoiceDetail (Cascade Delete)
            builder.HasMany(t => t.EInvoiceDetails)
                .WithOne(d => d.EInvoiceHeader)
                .HasForeignKey(d => d.EInvoiceHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Same-module FK → EWaybillHeader (Restrict — EWaybill must be deleted first)
            builder.HasMany(t => t.EWaybillHeaders)
                .WithOne(w => w.EInvoiceHeader)
                .HasForeignKey(w => w.EInvoiceHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.IrnNumber).IsUnique();
            builder.HasIndex(t => t.InvoiceNo);
            builder.HasIndex(t => t.PartyId);
            builder.HasIndex(t => t.UnitId);
            builder.HasIndex(t => t.StatusId);
            builder.HasIndex(t => t.IrnStatus);
        }
    }
}
