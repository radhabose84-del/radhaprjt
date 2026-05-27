using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QCManagement.Domain.Entities;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Infrastructure.Data.Configurations
{
    public class QualityTemplateParameterConfiguration : IEntityTypeConfiguration<QualityTemplateParameter>
    {
        public void Configure(EntityTypeBuilder<QualityTemplateParameter> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("QualityTemplateParameter", "QC");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.QualityTemplateId)
                .HasColumnName("QualityTemplateId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.QualityParameterId)
                .HasColumnName("QualityParameterId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.SequenceNo)
                .HasColumnName("SequenceNo")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.IsMandatory)
                .HasColumnName("IsMandatory")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(t => t.IsCritical)
                .HasColumnName("IsCritical")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(t => t.InspectionMethodId)
                .HasColumnName("InspectionMethodId")
                .HasColumnType("int");

            builder.Property(t => t.SampleSize)
                .HasColumnName("SampleSize")
                .HasColumnType("int");

            builder.Property(t => t.SampleUomId)
                .HasColumnName("SampleUomId")
                .HasColumnType("int");

            builder.Property(t => t.IsGradeApplicable)
                .HasColumnName("IsGradeApplicable")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(t => t.Remarks)
                .HasColumnName("Remarks")
                .HasColumnType("varchar(500)");

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

            // Audit fields
            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Indexes
            builder.HasIndex(t => t.QualityTemplateId);
            builder.HasIndex(t => t.QualityParameterId);
            builder.HasIndex(t => t.InspectionMethodId);
            builder.HasIndex(t => t.SampleUomId);
            // Composite unique: a parameter can appear at most once per template
            builder.HasIndex(t => new { t.QualityTemplateId, t.QualityParameterId }).IsUnique();

            // FK: QualityTemplateParameter → QualityTemplate (parent header)
            builder.HasOne(t => t.QualityTemplate)
                .WithMany(d => d!.QualityTemplateParameters)
                .HasForeignKey(t => t.QualityTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: QualityTemplateParameter → QualityParameter (same module)
            builder.HasOne(t => t.QualityParameter)
                .WithMany(d => d!.QualityTemplateParameters)
                .HasForeignKey(t => t.QualityParameterId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: QualityTemplateParameter → MiscMaster (InspectionMethod, same module, nullable)
            builder.HasOne(t => t.InspectionMethod)
                .WithMany(m => m!.QualityTemplateParametersAsInspectionMethod)
                .HasForeignKey(t => t.InspectionMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            // Note: SampleUomId references Inventory.UOM (cross-module) — no DB FK constraint per CLAUDE.md
        }
    }
}
