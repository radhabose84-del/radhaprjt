using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class EWaybillHeaderConfiguration : IEntityTypeConfiguration<EWaybillHeader>
    {
        public void Configure(EntityTypeBuilder<EWaybillHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("EWaybillHeader", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnType("int").IsRequired();
            builder.Property(t => t.EInvoiceHeaderId).HasColumnType("int").IsRequired(false);
            builder.Property(t => t.UnitId).HasColumnType("int").IsRequired();
            builder.Property(t => t.EWBNumber).HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(t => t.InvoiceNo).HasColumnType("varchar(30)").IsRequired(false);
            builder.Property(t => t.InvoiceDate).HasColumnType("date").IsRequired(false);
            builder.Property(t => t.InvoiceValue).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.SupplyType).HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(t => t.SubSupplyType).HasColumnType("varchar(30)").IsRequired(false);
            builder.Property(t => t.DocumentType).HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(t => t.TransactionType).HasColumnType("int").IsRequired(false);
            builder.Property(t => t.FromGSTIN).HasColumnType("varchar(15)").IsRequired(false);
            builder.Property(t => t.FromTradeName).HasColumnType("varchar(100)").IsRequired(false);
            builder.Property(t => t.ToGSTIN).HasColumnType("varchar(15)").IsRequired(false);
            builder.Property(t => t.ToTradeName).HasColumnType("varchar(100)").IsRequired(false);
            builder.Property(t => t.TotalValue).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.CGST).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.SGST).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.IGST).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.Cess).HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TransporterId).HasColumnType("int").IsRequired(false);
            builder.Property(t => t.TransporterGSTIN).HasColumnType("varchar(15)").IsRequired(false);
            builder.Property(t => t.TransporterName).HasColumnType("varchar(200)").IsRequired(false);
            builder.Property(t => t.TransportMode).HasColumnType("varchar(5)").IsRequired(false);
            builder.Property(t => t.TransDocNo).HasColumnType("varchar(30)").IsRequired(false);
            builder.Property(t => t.TransDocDate).HasColumnType("date").IsRequired(false);
            builder.Property(t => t.VehicleNo).HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(t => t.VehicleType).HasColumnType("varchar(5)").IsRequired(false);
            builder.Property(t => t.Distance).HasColumnType("int").IsRequired(false);
            builder.Property(t => t.PartyId).HasColumnType("int").IsRequired(false);
            builder.Property(t => t.GeneratedDate).IsRequired(false);
            builder.Property(t => t.ValidUpto).IsRequired(false);
            builder.Property(t => t.EwbStatus).HasColumnType("varchar(20)").HasDefaultValue("Pending").IsRequired(false);
            builder.Property(t => t.ErrorCode).HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(t => t.ErrorMessage).HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(t => t.CancelledDate).IsRequired(false);
            builder.Property(t => t.CancelReason).HasColumnType("varchar(500)").IsRequired(false);

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

            // Same-module FK → EWaybillDetail (Cascade Delete)
            builder.HasMany(t => t.EWaybillDetails)
                .WithOne(d => d.EWaybillHeader)
                .HasForeignKey(d => d.EWaybillHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => t.EWBNumber).IsUnique();
            builder.HasIndex(t => t.EInvoiceHeaderId);
            builder.HasIndex(t => t.UnitId);
            builder.HasIndex(t => t.PartyId);
            builder.HasIndex(t => t.TransporterId);
            builder.HasIndex(t => t.EwbStatus);
        }
    }
}
