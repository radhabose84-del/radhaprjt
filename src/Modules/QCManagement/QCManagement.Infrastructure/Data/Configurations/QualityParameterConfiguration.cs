using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QCManagement.Domain.Entities;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Infrastructure.Data.Configurations
{
    public class QualityParameterConfiguration : IEntityTypeConfiguration<QualityParameter>
    {
        public void Configure(EntityTypeBuilder<QualityParameter> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("QualityParameter", "QC");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.ParameterCode)
                .HasColumnName("ParameterCode")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.ParameterName)
                .HasColumnName("ParameterName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.ParameterGroupId)
                .HasColumnName("ParameterGroupId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DataTypeId)
                .HasColumnName("DataTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int");

            builder.Property(t => t.ValidationTypeId)
                .HasColumnName("ValidationTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnName("Description")
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

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            builder.HasIndex(t => t.ParameterCode).IsUnique();
            builder.HasIndex(t => t.ParameterName);
            builder.HasIndex(t => t.ParameterGroupId);
            builder.HasIndex(t => t.DataTypeId);
            builder.HasIndex(t => t.UnitId);
            builder.HasIndex(t => t.ValidationTypeId);

            builder.HasOne(t => t.ParameterGroup)
                .WithMany(m => m!.QualityParametersAsParameterGroup)
                .HasForeignKey(t => t.ParameterGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.DataType)
                .WithMany(m => m!.QualityParametersAsDataType)
                .HasForeignKey(t => t.DataTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ValidationType)
                .WithMany(m => m!.QualityParametersAsValidationType)
                .HasForeignKey(t => t.ValidationTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
