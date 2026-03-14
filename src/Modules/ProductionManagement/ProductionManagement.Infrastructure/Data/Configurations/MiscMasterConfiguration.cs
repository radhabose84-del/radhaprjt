using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductionManagement.Domain.Entities;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Data.Configurations
{
    public class MiscMasterConfiguration : IEntityTypeConfiguration<MiscMaster>
    {
        public void Configure(EntityTypeBuilder<MiscMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("MiscMaster", "Production");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.MiscTypeId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Code)
                .HasColumnType("varchar(100)")
                .IsRequired(false);

            builder.Property(t => t.Description)
                .HasColumnType("varchar(200)")
                .IsRequired(false);

            builder.Property(t => t.SortOrder)
                .HasColumnType("int")
                .IsRequired();

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

            builder.HasIndex(t => new { t.MiscTypeId, t.Code }).IsUnique();
            builder.HasIndex(t => t.MiscTypeId);

            // FK: MiscMaster → MiscTypeMaster (same-module)
            builder.HasOne(t => t.MiscTypeMaster)
                .WithMany(m => m.MiscMasters)
                .HasForeignKey(t => t.MiscTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
