using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations
{
    public class BarcodeAllocationConfiguration : IEntityTypeConfiguration<BarcodeAllocation>
    {
        public void Configure(EntityTypeBuilder<BarcodeAllocation> builder)
        {
            // ValueConverter for Status (enum to bit)
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            // ValueConverter for IsDelete (enum to bit)
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("BarcodeAllocation", "Purchase");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.AllocationNumber)
                .HasColumnName("AllocationNumber")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(b => b.AllocationDate)
                .HasColumnName("AllocationDate")
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(b => b.EmployeeNo)
                .HasColumnName("EmployeeNo")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(b => b.EmployeeName)
                .HasColumnName("EmployeeName")
                .HasColumnType("varchar(150)")
                .IsRequired();

            builder.Property(b => b.BarcodeSeriesId)
                .HasColumnName("BarcodeSeriesId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.BarcodeFrom)
                .HasColumnName("BarcodeFrom")
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(b => b.BarcodeTo)
                .HasColumnName("BarcodeTo")
                .HasColumnType("bigint")
                .IsRequired();

            builder.Property(b => b.UsedQuantity)
                .HasColumnName("UsedQuantity")
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(b => b.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(b => b.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("varchar(250)");

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

            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(20)");

            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(20)");

            // Indexes
            builder.HasIndex(b => b.AllocationNumber).IsUnique();
            builder.HasIndex(b => b.BarcodeSeriesId);
            builder.HasIndex(b => b.StatusId);

            // Same-module FK -> Purchase.BarcodeSeries (the source series)
            builder.HasOne(b => b.BarcodeSeries)
                .WithMany(s => s.Allocations)
                .HasForeignKey(b => b.BarcodeSeriesId)
                .HasPrincipalKey(s => s.Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK -> Purchase.MiscMaster (Status)
            builder.HasOne(b => b.Status)
                .WithMany(m => m.BarcodeAllocationStatuses)
                .HasForeignKey(b => b.StatusId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
