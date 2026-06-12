using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductionManagement.Domain.Entities;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Data.Configurations
{
    public class YarnTypeConfiguration : IEntityTypeConfiguration<YarnType>
    {
        public void Configure(EntityTypeBuilder<YarnType> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("YarnType", "Production");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.YarnTypeCode)
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.YarnTypeName)
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnType("varchar(200)");

            builder.Property(t => t.AdditionalPrice)
                .HasColumnType("decimal(18,4)");

            // Cross-module FK to UserManagement.Currency — no DB constraint (resolved via ICurrencyLookup)
            builder.Property(t => t.CurrencyId)
                .HasColumnType("int");

            builder.Property(b => b.IsActive)
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnType("int").IsRequired();
            builder.Property(t => t.CreatedDate).HasColumnType("datetimeoffset");
            builder.Property(t => t.CreatedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnType("datetimeoffset");
            builder.Property(t => t.ModifiedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnType("varchar(50)");

            builder.HasIndex(t => t.YarnTypeCode).IsUnique();
            builder.HasIndex(t => t.YarnTypeName).IsUnique();
            builder.HasIndex(t => t.CurrencyId);
        }
    }
}
