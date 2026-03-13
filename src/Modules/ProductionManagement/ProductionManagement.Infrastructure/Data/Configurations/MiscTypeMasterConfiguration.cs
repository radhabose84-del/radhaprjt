using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductionManagement.Domain.Entities;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Data.Configurations
{
    public class MiscTypeMasterConfiguration : IEntityTypeConfiguration<MiscTypeMaster>
    {
        public void Configure(EntityTypeBuilder<MiscTypeMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("MiscTypeMaster", "Production");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.MiscTypeCode)
                .HasColumnType("varchar(50)")
                .IsRequired(false);

            builder.Property(t => t.Description)
                .HasColumnType("varchar(200)")
                .IsRequired(false);

            builder.Property(b => b.IsActive)
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnType("datetimeoffset");
            builder.Property(t => t.CreatedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnType("datetimeoffset");
            builder.Property(t => t.ModifiedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnType("varchar(50)");

            builder.HasIndex(t => t.MiscTypeCode).IsUnique();
        }
    }
}
