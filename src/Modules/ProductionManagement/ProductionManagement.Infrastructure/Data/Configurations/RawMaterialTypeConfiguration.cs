using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductionManagement.Domain.Entities;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Data.Configurations
{
    public class RawMaterialTypeConfiguration : IEntityTypeConfiguration<RawMaterialType>
    {
        public void Configure(EntityTypeBuilder<RawMaterialType> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("RawMaterialType", "Production");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.RawMaterialTypeCode)
                .HasColumnName("RawMaterialTypeCode")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.RawMaterialTypeName)
                .HasColumnName("RawMaterialTypeName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(255)");

            builder.Property(t => t.EffectiveFrom)
                .HasColumnName("EffectiveFrom")
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(t => t.EffectiveTo)
                .HasColumnName("EffectiveTo")
                .HasColumnType("datetimeoffset");

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

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int").IsRequired();
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate").HasColumnType("datetimeoffset");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate").HasColumnType("datetimeoffset");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // ⭐ Soft-delete-safe filtered unique indexes (SalesChannel bug fix — see CLAUDE.md Rule #26).
            // Without HasFilter, soft-deleted rows would still occupy the unique slot, blocking re-creation
            // of a Code/Name whose only previous occupant was soft-deleted (the user has no UI path to
            // recover those tombstone rows). The filter aligns the SQL constraint with the application's
            // soft-delete semantics — references: PortMasterConfiguration.cs, SalesSegmentConfiguration.cs,
            // PriceGroupMasterConfiguration.cs.
            builder.HasIndex(t => t.RawMaterialTypeCode)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(t => t.RawMaterialTypeName)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // Non-unique helper index — no filter needed (not a constraint).
            builder.HasIndex(t => t.EffectiveFrom);
        }
    }
}
