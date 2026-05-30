using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QCManagement.Domain.Entities;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Infrastructure.Data.Configurations
{
    public class QcInspectionDtlConfiguration : IEntityTypeConfiguration<QcInspectionDtl>
    {
        public void Configure(EntityTypeBuilder<QcInspectionDtl> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("QcInspectionDtl", "QC");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.QcInspectionHdrId).HasColumnName("QcInspectionHdrId").HasColumnType("int").IsRequired();

            builder.Property(t => t.QualitySpecificationParameterId).HasColumnName("QualitySpecificationParameterId").HasColumnType("int").IsRequired();
            builder.Property(t => t.QualityParameterId).HasColumnName("QualityParameterId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ParameterCode).HasColumnName("ParameterCode").HasColumnType("varchar(20)").IsRequired();
            builder.Property(t => t.ParameterName).HasColumnName("ParameterName").HasColumnType("varchar(100)").IsRequired();
            builder.Property(t => t.DataTypeId).HasColumnName("DataTypeId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ValidationTypeId).HasColumnName("ValidationTypeId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ValidationTypeCode).HasColumnName("ValidationTypeCode").HasColumnType("varchar(20)").IsRequired();
            builder.Property(t => t.UomId).HasColumnName("UomId").HasColumnType("int");
            builder.Property(t => t.UomCode).HasColumnName("UomCode").HasColumnType("varchar(20)");
            builder.Property(t => t.MinValue).HasColumnName("MinValue").HasColumnType("decimal(18,4)");
            builder.Property(t => t.MaxValue).HasColumnName("MaxValue").HasColumnType("decimal(18,4)");
            builder.Property(t => t.ExpectedValue).HasColumnName("ExpectedValue").HasColumnType("varchar(200)");
            builder.Property(t => t.AllowedValues).HasColumnName("AllowedValues").HasColumnType("varchar(2000)");
            builder.Property(t => t.SeverityId).HasColumnName("SeverityId").HasColumnType("int").IsRequired();
            builder.Property(t => t.SeverityCode).HasColumnName("SeverityCode").HasColumnType("varchar(20)").IsRequired();
            builder.Property(t => t.FailureActionId).HasColumnName("FailureActionId").HasColumnType("int").IsRequired();
            builder.Property(t => t.SortOrder).HasColumnName("SortOrder").HasColumnType("int").IsRequired();

            builder.Property(t => t.ActualValue).HasColumnName("ActualValue").HasColumnType("varchar(200)");
            builder.Property(t => t.InspectionResult).HasColumnName("InspectionResult").HasColumnType("varchar(10)");
            builder.Property(t => t.Remarks).HasColumnName("Remarks").HasColumnType("varchar(500)");

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

            builder.HasIndex(t => new { t.QcInspectionHdrId, t.SortOrder });

            // FK: QcInspectionDtl → QcInspectionHdr (same module, cascade delete of child rows)
            builder.HasOne(t => t.Hdr)
                .WithMany(h => h!.Details)
                .HasForeignKey(t => t.QcInspectionHdrId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
