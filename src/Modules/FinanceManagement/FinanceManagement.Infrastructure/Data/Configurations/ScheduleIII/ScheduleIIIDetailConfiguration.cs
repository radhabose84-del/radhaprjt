using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class ScheduleIIIDetailConfiguration : IEntityTypeConfiguration<ScheduleIIIDetail>
    {
        public void Configure(EntityTypeBuilder<ScheduleIIIDetail> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("ScheduleIIIDetail", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.ScheduleIIIHeaderId)
                .HasColumnName("ScheduleIIIHeaderId").HasColumnType("int").IsRequired();

            builder.Property(t => t.ScheduleIIISectionId)
                .HasColumnName("ScheduleIIISectionId").HasColumnType("int").IsRequired();

            builder.Property(t => t.ScheduleIIISectionItemId)
                .HasColumnName("ScheduleIIISectionItemId").HasColumnType("int").IsRequired();

            builder.Property(t => t.DisplayOrder)
                .HasColumnName("DisplayOrder").HasColumnType("int")
                .HasDefaultValue(0).IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive").HasColumnType("bit")
                .HasConversion(statusConverter).IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted").HasColumnType("bit")
                .HasConversion(isDeleteConverter).IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Same-module FK -> ScheduleIIIHeader (cascade-restricted; soft delete used)
            builder.HasOne(t => t.Header)
                .WithMany(h => h.Details)
                .HasForeignKey(t => t.ScheduleIIIHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK -> ScheduleIIISection
            builder.HasOne(t => t.Section)
                .WithMany()
                .HasForeignKey(t => t.ScheduleIIISectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK -> ScheduleIIISectionItem (the included line)
            builder.HasOne(t => t.ScheduleIIISectionItem)
                .WithMany(l => l.DetailRows)
                .HasForeignKey(t => t.ScheduleIIISectionItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // A line appears at most once per header, and display order is unique per header (active rows).
            builder.HasIndex(t => new { t.ScheduleIIIHeaderId, t.ScheduleIIISectionItemId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(t => new { t.ScheduleIIIHeaderId, t.DisplayOrder })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(t => t.ScheduleIIISectionId);
            builder.HasIndex(t => t.ScheduleIIISectionItemId);
        }
    }
}
