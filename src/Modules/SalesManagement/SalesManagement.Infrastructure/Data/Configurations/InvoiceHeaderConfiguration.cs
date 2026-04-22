using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class InvoiceHeaderConfiguration : IEntityTypeConfiguration<InvoiceHeader>
    {
        public void Configure(EntityTypeBuilder<InvoiceHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v)
            );

            var nullableDateOnlyConverter = new ValueConverter<DateOnly?, DateTime?>(
                v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                v => v.HasValue ? DateOnly.FromDateTime(v.Value) : (DateOnly?)null
            );

            builder.ToTable("InvoiceHeader", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.InvoiceNo).HasColumnName("InvoiceNo").HasColumnType("varchar(30)").IsRequired(false);
            builder.Property(t => t.InvoiceDate).HasColumnName("InvoiceDate").HasColumnType("date").HasConversion(dateOnlyConverter).IsRequired();
            builder.Property(t => t.DispatchAdviceId).HasColumnName("DispatchAdviceId").HasColumnType("int").IsRequired();
            builder.Property(t => t.PartyId).HasColumnName("PartyId").HasColumnType("int").IsRequired();
            builder.Property(t => t.AgentId).HasColumnName("AgentId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.UnitId).HasColumnName("UnitId").HasColumnType("int").IsRequired();
            builder.Property(t => t.FinancialYearId).HasColumnName("FinancialYearId").HasColumnType("int").IsRequired();
            builder.Property(t => t.InvoiceTypeId).HasColumnName("InvoiceTypeId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.TransportMode).HasColumnName("TransportMode").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.StatusId).HasColumnName("StatusId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.VehicleNumber).HasColumnName("VehicleNumber").HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(t => t.TransporterName).HasColumnName("TransporterName").HasColumnType("varchar(100)").IsRequired(false);
            builder.Property(t => t.LRNumber).HasColumnName("LRNumber").HasColumnType("varchar(50)").IsRequired(false);
            builder.Property(t => t.LRDate).HasColumnName("LRDate").HasColumnType("date").HasConversion(nullableDateOnlyConverter).IsRequired(false);
            builder.Property(t => t.TotalBags).HasColumnName("TotalBags").HasColumnType("int").IsRequired();
            builder.Property(t => t.TotalWeight).HasColumnName("TotalWeight").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TaxableValue).HasColumnName("TaxableValue").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TotalDiscount).HasColumnName("TotalDiscount").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TotalFreight).HasColumnName("TotalFreight").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TotalCommission).HasColumnName("TotalCommission").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.Insurance).HasColumnName("Insurance").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.HandlingCharge).HasColumnName("HandlingCharge").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TotalCharity).HasColumnName("TotalCharity").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.OtherCharges).HasColumnName("OtherCharges").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.CGST).HasColumnName("CGST").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.SGST).HasColumnName("SGST").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.IGST).HasColumnName("IGST").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TaxAmount).HasColumnName("TaxAmount").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TCSPercentage).HasColumnName("TCSPercentage").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.TCS).HasColumnName("TCS").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.RoundOff).HasColumnName("RoundOff").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.InvoiceAmountBeforeTCS).HasColumnName("InvoiceAmountBeforeTCS").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.InvoiceAmount).HasColumnName("InvoiceAmount").HasColumnType("decimal(18,6)").IsRequired();
            builder.Property(t => t.Remarks).HasColumnName("Remarks").HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(t => t.GEFlag).HasColumnName("GEFlag").HasColumnType("bit").HasDefaultValue(false).IsRequired();

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

            // Same-module FK constraints
            builder.HasOne(t => t.TransportModeMisc)
                .WithMany(m => m.InvoiceHeadersAsTransportMode)
                .HasForeignKey(t => t.TransportMode)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.StatusMisc)
                .WithMany(m => m.InvoiceHeadersAsStatus)
                .HasForeignKey(t => t.StatusId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.DispatchAdviceHeader)
                .WithMany(d => d.InvoiceHeaders)
                .HasForeignKey(t => t.DispatchAdviceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Child collection
            builder.HasMany(t => t.InvoiceDetails)
                .WithOne(d => d.InvoiceHeader)
                .HasForeignKey(d => d.InvoiceHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => t.InvoiceNo).IsUnique();
            builder.HasIndex(t => t.InvoiceTypeId);
            builder.HasIndex(t => t.DispatchAdviceId);
            builder.HasIndex(t => t.PartyId);
            builder.HasIndex(t => t.InvoiceDate);
            builder.HasIndex(t => t.FinancialYearId);
            builder.HasIndex(t => t.UnitId);
        }
    }
}
