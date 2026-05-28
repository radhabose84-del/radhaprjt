using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QCManagement.Domain.Entities;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Infrastructure.Data.Configurations
{
    public class QualitySpecificationParameterConfiguration : IEntityTypeConfiguration<QualitySpecificationParameter>
    {
        public void Configure(EntityTypeBuilder<QualitySpecificationParameter> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("QualitySpecificationParameter", "QC");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.QualitySpecificationId)
                .HasColumnName("QualitySpecificationId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.QualityParameterId)
                .HasColumnName("QualityParameterId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ValidationTypeId)
                .HasColumnName("ValidationTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.MinValue)
                .HasColumnName("MinValue")
                .HasColumnType("decimal(18,6)");

            builder.Property(t => t.MaxValue)
                .HasColumnName("MaxValue")
                .HasColumnType("decimal(18,6)");

            builder.Property(t => t.ExpectedValue)
                .HasColumnName("ExpectedValue")
                .HasColumnType("varchar(250)");

            builder.Property(t => t.AllowedValues)
                .HasColumnName("AllowedValues")
                .HasColumnType("varchar(2000)");

            builder.Property(t => t.SeverityId)
                .HasColumnName("SeverityId")
                .HasColumnType("int");

            builder.Property(t => t.FailureActionId)
                .HasColumnName("FailureActionId")
                .HasColumnType("int");

            builder.Property(t => t.IsSamplingRequired)
                .HasColumnName("IsSamplingRequired")
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
            builder.HasIndex(t => t.QualitySpecificationId);
            builder.HasIndex(t => t.QualityParameterId);
            builder.HasIndex(t => t.ValidationTypeId);
            builder.HasIndex(t => t.SeverityId);
            builder.HasIndex(t => t.FailureActionId);
            // Composite unique: a parameter can appear at most once per specification
            builder.HasIndex(t => new { t.QualitySpecificationId, t.QualityParameterId }).IsUnique();

            // FK: QualitySpecificationParameter → QualitySpecification (parent header)
            builder.HasOne(t => t.QualitySpecification)
                .WithMany(d => d!.QualitySpecificationParameters)
                .HasForeignKey(t => t.QualitySpecificationId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: QualitySpecificationParameter → QualityParameter (same module)
            builder.HasOne(t => t.QualityParameter)
                .WithMany(d => d!.QualitySpecificationParameters)
                .HasForeignKey(t => t.QualityParameterId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: QualitySpecificationParameter → MiscMaster (ValidationType, same module)
            builder.HasOne(t => t.ValidationType)
                .WithMany(m => m!.QualitySpecificationParametersAsValidationType)
                .HasForeignKey(t => t.ValidationTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: QualitySpecificationParameter → MiscMaster (Severity, same module, nullable)
            builder.HasOne(t => t.Severity)
                .WithMany(m => m!.QualitySpecificationParametersAsSeverity)
                .HasForeignKey(t => t.SeverityId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: QualitySpecificationParameter → MiscMaster (FailureAction, same module, nullable)
            builder.HasOne(t => t.FailureAction)
                .WithMany(m => m!.QualitySpecificationParametersAsFailureAction)
                .HasForeignKey(t => t.FailureActionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
