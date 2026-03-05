using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class DispatchAdviceHeaderConfiguration : IEntityTypeConfiguration<DispatchAdviceHeader>
    {
        public void Configure(EntityTypeBuilder<DispatchAdviceHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("DispatchAdviceHeader", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DispatchNo)
                .HasColumnName("DispatchNo")
                .HasColumnType("varchar(30)")
                .IsRequired();

            builder.Property(t => t.DispatchDate)
                .HasColumnName("DispatchDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SalesOrderId)
                .HasColumnName("SalesOrderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.PartyId)
                .HasColumnName("PartyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TotOrderQty)
                .HasColumnName("TotOrderQty")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TotDispatchedQty)
                .HasColumnName("TotDispatchedQty")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.TotPendingQty)
                .HasColumnName("TotPendingQty")
                .HasColumnType("decimal(18,3)")
                .IsRequired();

            builder.Property(t => t.DispatchAddressId)
                .HasColumnName("DispatchAddressId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TransporterId)
                .HasColumnName("TransporterId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.VehicleNo)
                .HasColumnName("VehicleNo")
                .HasColumnType("varchar(20)")
                .IsRequired(false);

            builder.Property(t => t.DriverName)
                .HasColumnName("DriverName")
                .HasColumnType("varchar(100)")
                .IsRequired(false);

            builder.Property(t => t.LRNo)
                .HasColumnName("LRNo")
                .HasColumnType("varchar(50)")
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
            builder.HasOne(t => t.StatusMisc)
                .WithMany(m => m.DispatchAdviceHeadersAsStatus)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.SalesOrderHeader)
                .WithMany(h => h.DispatchAdviceHeaders)
                .HasForeignKey(t => t.SalesOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.DispatchAddress)
                .WithMany(d => d.DispatchAdviceHeaders)
                .HasForeignKey(t => t.DispatchAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // Child collection — Header → Details
            builder.HasMany(t => t.DispatchAdviceDetails)
                .WithOne(d => d.DispatchAdviceHeader)
                .HasForeignKey(d => d.DispatchAdviceHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.DispatchNo).IsUnique();
            builder.HasIndex(t => t.SalesOrderId);
            builder.HasIndex(t => t.PartyId);
            builder.HasIndex(t => t.StatusId);
            builder.HasIndex(t => t.DispatchDate);
        }
    }
}
