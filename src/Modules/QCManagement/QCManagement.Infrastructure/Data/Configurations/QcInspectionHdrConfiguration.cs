using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QCManagement.Domain.Entities;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Infrastructure.Data.Configurations
{
    public class QcInspectionHdrConfiguration : IEntityTypeConfiguration<QcInspectionHdr>
    {
        public void Configure(EntityTypeBuilder<QcInspectionHdr> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("QcInspectionHdr", "QC");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.QcInspectionNo)
                .HasColumnName("QcInspectionNo").HasColumnType("varchar(20)").IsRequired();

            builder.Property(t => t.InspectionDate)
                .HasColumnName("InspectionDate").HasColumnType("datetimeoffset").IsRequired();

            builder.Property(t => t.GrnHeaderId).HasColumnName("GrnHeaderId").HasColumnType("int").IsRequired();
            builder.Property(t => t.GrnDetailId).HasColumnName("GrnDetailId").HasColumnType("int").IsRequired();

            builder.Property(t => t.QualitySpecificationId).HasColumnName("QualitySpecificationId").HasColumnType("int").IsRequired();
            builder.Property(t => t.QualitySpecificationCode).HasColumnName("QualitySpecificationCode").HasColumnType("varchar(20)").IsRequired();
            builder.Property(t => t.QualityTemplateId).HasColumnName("QualityTemplateId").HasColumnType("int").IsRequired();
            builder.Property(t => t.QualityTemplateCode).HasColumnName("QualityTemplateCode").HasColumnType("varchar(20)").IsRequired();
            builder.Property(t => t.QcTypeId).HasColumnName("QcTypeId").HasColumnType("int").IsRequired();

            builder.Property(t => t.InspectorUserId).HasColumnName("InspectorUserId").HasColumnType("int").IsRequired();
            builder.Property(t => t.InspectorName).HasColumnName("InspectorName").HasColumnType("varchar(100)").IsRequired();

            builder.Property(t => t.ReceivedQuantity).HasColumnName("ReceivedQuantity").HasColumnType("decimal(18,3)").IsRequired();
            builder.Property(t => t.ReceivedUomId).HasColumnName("ReceivedUomId").HasColumnType("int").IsRequired();
            builder.Property(t => t.BatchNumber).HasColumnName("BatchNumber").HasColumnType("varchar(50)");
            builder.Property(t => t.LotNumber).HasColumnName("LotNumber").HasColumnType("varchar(50)");

            builder.Property(t => t.QcStatusId).HasColumnName("QcStatusId").HasColumnType("int");
            builder.Property(t => t.AcceptedQuantity).HasColumnName("AcceptedQuantity").HasColumnType("decimal(18,3)");
            builder.Property(t => t.RejectedQuantity).HasColumnName("RejectedQuantity").HasColumnType("decimal(18,3)");
            builder.Property(t => t.DispositionRemarks).HasColumnName("DispositionRemarks").HasColumnType("varchar(500)");
            builder.Property(t => t.DispositionDate).HasColumnName("DispositionDate").HasColumnType("datetimeoffset");
            builder.Property(t => t.DispositionByUserId).HasColumnName("DispositionByUserId").HasColumnType("int");
            builder.Property(t => t.DispositionByName).HasColumnName("DispositionByName").HasColumnType("varchar(100)");

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.QcInspectionNo).IsUnique();
            builder.HasIndex(t => t.GrnDetailId).IsUnique().HasFilter("[IsDeleted] = 0");
            builder.HasIndex(t => t.GrnHeaderId);
            builder.HasIndex(t => new { t.InspectionDate, t.QcStatusId });
            builder.HasIndex(t => t.BatchNumber);

            // Same-module FK: QcStatusId → QC.MiscMaster (optional, no reverse navigation)
            builder.HasOne(t => t.QcStatus)
                .WithMany()
                .HasForeignKey(t => t.QcStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Snapshot FKs (QualitySpecificationId, QualityTemplateId, QcTypeId) and cross-module FKs
            // (GrnHeaderId, GrnDetailId, ReceivedUomId) have NO DB constraint per CLAUDE.md.
        }
    }
}
