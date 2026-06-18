using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class GlAccountImportErrorConfiguration : IEntityTypeConfiguration<GlAccountImportError>
    {
        public void Configure(EntityTypeBuilder<GlAccountImportError> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("GlAccountImportError", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.ImportLogId).HasColumnName("ImportLogId").HasColumnType("int").IsRequired();
            builder.Property(t => t.RowNumber).HasColumnName("RowNumber").HasColumnType("int").IsRequired();
            builder.Property(t => t.RecordType).HasColumnName("RecordType").HasColumnType("varchar(10)");
            builder.Property(t => t.ColumnName).HasColumnName("ColumnName").HasColumnType("varchar(100)");
            builder.Property(t => t.AttemptedValue).HasColumnName("AttemptedValue").HasColumnType("varchar(200)");
            builder.Property(t => t.ErrorMessage).HasColumnName("ErrorMessage").HasColumnType("varchar(500)").IsRequired();

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

            builder.HasIndex(t => t.ImportLogId);

            // Relationship is configured from the GlAccountImportLog side (HasMany → WithOne).
        }
    }
}
