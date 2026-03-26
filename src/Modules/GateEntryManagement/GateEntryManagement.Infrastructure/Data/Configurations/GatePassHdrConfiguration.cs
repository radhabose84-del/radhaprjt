using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GateEntryManagement.Domain.Entities;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.Infrastructure.Data.Configurations
{
    public class GatePassHdrConfiguration : IEntityTypeConfiguration<GatePassHdr>
    {
        public void Configure(EntityTypeBuilder<GatePassHdr> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("GatePassHdr", "Gate");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.GatePassNo)
                .HasColumnName("GatePassNo")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.GatePassDate)
                .HasColumnName("GatePassDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(t => t.VehicleMovementRecordId)
                .HasColumnName("VehicleMovementRecordId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.VehicleNumber)
                .HasColumnName("VehicleNumber")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.DriverName)
                .HasColumnName("DriverName")
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            builder.Property(t => t.DriverMobile)
                .HasColumnName("DriverMobile")
                .HasColumnType("varchar(10)")
                .IsRequired(false);

            builder.Property(t => t.TransporterName)
                .HasColumnName("TransporterName")
                .HasColumnType("varchar(100)")
                .IsRequired(false);

            builder.Property(t => t.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TotalItems)
                .HasColumnName("TotalItems")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.TotalDocumentQty)
                .HasColumnName("TotalDocumentQty")
                .HasColumnType("decimal(18,6)")
                .IsRequired();

            builder.Property(t => t.TotalDispatchQty)
                .HasColumnName("TotalDispatchQty")
                .HasColumnType("decimal(18,6)")
                .IsRequired();

            builder.Property(t => t.ReturnableItems)
                .HasColumnName("ReturnableItems")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.TotalValue)
                .HasColumnName("TotalValue")
                .HasColumnType("decimal(18,6)")
                .IsRequired();

            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("varchar(500)")
                .IsRequired(false);

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

            // Indexes
            builder.HasIndex(t => t.GatePassNo).IsUnique();
            builder.HasIndex(t => t.VehicleMovementRecordId);
            builder.HasIndex(t => t.UnitId);

            // Same-module FK
            builder.HasOne(t => t.VehicleMovementRecord)
                .WithMany(v => v.GatePassHeaders)
                .HasForeignKey(t => t.VehicleMovementRecordId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
