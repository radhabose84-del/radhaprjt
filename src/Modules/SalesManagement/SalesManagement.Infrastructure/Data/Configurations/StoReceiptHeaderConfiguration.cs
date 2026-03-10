using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class StoReceiptHeaderConfiguration : IEntityTypeConfiguration<StoReceiptHeader>
    {
        public void Configure(EntityTypeBuilder<StoReceiptHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("StoReceiptHeader", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.StoReceiptNumber)
                .HasColumnName("StoReceiptNumber")
                .HasColumnType("varchar(30)")
                .IsRequired();

            builder.Property(t => t.StoReceiptDate)
                .HasColumnName("StoReceiptDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.DeliveryChallanHeaderId)
                .HasColumnName("DeliveryChallanHeaderId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ReceivingPlantId)
                .HasColumnName("ReceivingPlantId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ReceivingStorageLocationId)
                .HasColumnName("ReceivingStorageLocationId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.RackId)
                .HasColumnName("RackId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.VehicleNumber)
                .HasColumnName("VehicleNumber")
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired();

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

            // Same-module FK: StoReceiptHeader → DeliveryChallanHeader
            builder.HasOne(t => t.DeliveryChallanHeader)
                .WithMany(dc => dc.StoReceiptHeaders)
                .HasForeignKey(t => t.DeliveryChallanHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK: StoReceiptHeader → MiscMaster (Status)
            builder.HasOne(t => t.Status)
                .WithMany(m => m.StoReceiptHeadersAsStatus)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Child collection: StoReceiptHeader → StoReceiptDetails
            builder.HasMany(t => t.StoReceiptDetails)
                .WithOne(d => d.StoReceiptHeader)
                .HasForeignKey(d => d.StoReceiptHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.StoReceiptNumber).IsUnique();
            builder.HasIndex(t => t.DeliveryChallanHeaderId);
            builder.HasIndex(t => t.ReceivingPlantId);
            builder.HasIndex(t => t.StatusId);
            builder.HasIndex(t => t.StoReceiptDate);
        }
    }
}
