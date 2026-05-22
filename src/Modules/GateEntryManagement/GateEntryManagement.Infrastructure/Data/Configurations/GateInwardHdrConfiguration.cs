using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using GateEntryManagement.Domain.Entities;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.Infrastructure.Data.Configurations
{
    public class GateInwardHdrConfiguration : IEntityTypeConfiguration<GateInwardHdr>
    {
        public void Configure(EntityTypeBuilder<GateInwardHdr> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("GateInwardHdr", "Gate");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.GateEntryNo)
                .HasColumnName("GateEntryNo")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.VehicleMovementRecordId)
                .HasColumnName("VehicleMovementRecordId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ReceivingTypeId)
                .HasColumnName("ReceivingTypeId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.CourierNumber)
                .HasColumnName("CourierNumber")
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            // Cross-module FK (PartyManagement) — no DB FK constraint
            builder.Property(t => t.PartyId)
                .HasColumnName("PartyId")
                .HasColumnType("int")
                .IsRequired(false);

            // Weighbridge
            builder.Property(t => t.GrossWeight)
                .HasColumnName("GrossWeight")
                .HasColumnType("decimal(10,3)")
                .IsRequired(false);

            builder.Property(t => t.TareWeight)
                .HasColumnName("TareWeight")
                .HasColumnType("decimal(10,3)")
                .IsRequired(false);

            builder.Property(t => t.NetWeight)
                .HasColumnName("NetWeight")
                .HasColumnType("decimal(10,3)")
                .IsRequired(false);

            // QA
            builder.Property(t => t.QAInspectionRequired)
                .HasColumnName("QAInspectionRequired")
                .HasColumnType("bit")
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(t => t.QAStatusId)
                .HasColumnName("QAStatusId")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("varchar(250)")
                .IsRequired(false);

            // Single Gate Entry Document (optional)
            builder.Property(t => t.AttachmentFileName)
                .HasColumnName("AttachmentFileName")
                .HasColumnType("nvarchar(260)")
                .IsRequired(false);

            builder.Property(t => t.AttachmentFilePath)
                .HasColumnName("AttachmentFilePath")
                .HasColumnType("nvarchar(500)")
                .IsRequired(false);

            // Audit
            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive").HasColumnType("bit")
                .HasConversion(statusConverter).IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted").HasColumnType("bit")
                .HasConversion(isDeleteConverter).IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.GateEntryNo).IsUnique();
            builder.HasIndex(t => t.VehicleMovementRecordId);
            builder.HasIndex(t => t.UnitId);
            builder.HasIndex(t => t.PartyId);
            builder.HasIndex(t => t.ReceivingTypeId);

            // Same-module FKs
            builder.HasOne(t => t.VehicleMovementRecord)
                .WithMany(v => v.GateInwardHeaders)
                .HasForeignKey(t => t.VehicleMovementRecordId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.QAStatusMisc)
                .WithMany(m => m.GateInwardHdrsAsQAStatus)
                .HasForeignKey(t => t.QAStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ReceivingType)
                .WithMany(m => m.GateInwardHdrsAsReceivingType)
                .HasForeignKey(t => t.ReceivingTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
