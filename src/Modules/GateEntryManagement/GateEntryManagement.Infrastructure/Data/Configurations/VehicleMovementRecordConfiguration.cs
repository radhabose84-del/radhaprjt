using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GateEntryManagement.Domain.Entities;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.Infrastructure.Data.Configurations
{
    public class VehicleMovementRecordConfiguration : IEntityTypeConfiguration<VehicleMovementRecord>
    {
        public void Configure(EntityTypeBuilder<VehicleMovementRecord> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("VehicleMovementRecord", "Gate");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            // System Generated
            builder.Property(t => t.VehicleMovementId)
                .HasColumnName("VehicleMovementId")
                .HasColumnType("varchar(20)")
                .IsRequired();

            // Vehicle Details
            builder.Property(t => t.VehicleNumber)
                .HasColumnName("VehicleNumber")
                .HasColumnType("varchar(20)")
                .IsRequired(false); // Conditional — required only when ReceivingType = Vehicle (enforced by validator)

            builder.Property(t => t.DriverName)
                .HasColumnName("DriverName")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(t => t.DriverLicenseNo)
                .HasColumnName("DriverLicenseNo")
                .HasColumnType("varchar(25)")
                .IsRequired(false);

            builder.Property(t => t.DriverMobileNo)
                .HasColumnName("DriverMobileNo")
                .HasColumnType("varchar(10)")
                .IsRequired();

            builder.Property(t => t.TransporterId)
                .HasColumnName("TransporterId")
                .HasColumnType("int")
                .IsRequired(false);

            // Basic Information
            builder.Property(t => t.PurposeOfVisitId)
                .HasColumnName("PurposeOfVisitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ReceivingTypeId)
                .HasColumnName("ReceivingTypeId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ReferenceDocTypeId)
                .HasColumnName("ReferenceDocTypeId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.ReferenceDocNo)
                .HasColumnName("ReferenceDocNo")
                .HasColumnType("varchar(20)")
                .IsRequired(false);

            // System Fields
            builder.Property(t => t.GateInTime)
                .HasColumnName("GateInTime")
                .IsRequired();

            builder.Property(t => t.GateInBy)
                .HasColumnName("GateInBy")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(t => t.GateOutTime)
                .HasColumnName("GateOutTime")
                .IsRequired(false);

            builder.Property(t => t.GateOutBy)
                .HasColumnName("GateOutBy")
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("varchar(250)")
                .IsRequired(false);

            // Audit fields
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
            builder.HasIndex(t => t.VehicleMovementId).IsUnique();
            builder.HasIndex(t => t.VehicleNumber);
            builder.HasIndex(t => t.PurposeOfVisitId);
            builder.HasIndex(t => t.ReceivingTypeId);
            builder.HasIndex(t => t.StatusId);
            builder.HasIndex(t => t.UnitId);

            // Same-module FK constraints
            builder.HasOne(t => t.PurposeOfVisit)
                .WithMany(m => m.VehicleMovementRecordsAsPurposeOfVisit)
                .HasForeignKey(t => t.PurposeOfVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ReceivingType)
                .WithMany(m => m.VehicleMovementRecordsAsReceivingType)
                .HasForeignKey(t => t.ReceivingTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ReferenceDocType)
                .WithMany(m => m.VehicleMovementRecordsAsReferenceDocType)
                .HasForeignKey(t => t.ReferenceDocTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.StatusMisc)
                .WithMany(m => m.VehicleMovementRecordsAsStatus)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // No DB constraints for cross-module FKs (TransporterId, UnitId)
        }
    }
}
