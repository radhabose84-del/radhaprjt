using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class ScheduleIIIStructureConfiguration : IEntityTypeConfiguration<ScheduleIIIStructure>
    {
        public void Configure(EntityTypeBuilder<ScheduleIIIStructure> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("ScheduleIIIStructure", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            // Cross-module FK -> UserManagement Company (no DB constraint)
            builder.Property(t => t.CompanyId)
                .HasColumnName("CompanyId").HasColumnType("int").IsRequired();

            // Cross-module FK -> AppData.Division (no DB constraint)
            builder.Property(t => t.DivisionId)
                .HasColumnName("DivisionId").HasColumnType("int").IsRequired();

            // Same-module FK -> Finance.MiscMaster (S3_STATUS)
            builder.Property(t => t.StructureStatusId)
                .HasColumnName("StructureStatusId").HasColumnType("int").IsRequired();

            builder.Property(t => t.TextileSplitEnabled)
                .HasColumnName("TextileSplitEnabled").HasColumnType("bit")
                .HasDefaultValue(false).IsRequired();

            builder.Property(t => t.VersionNo)
                .HasColumnName("VersionNo").HasColumnType("int")
                .HasDefaultValue(1).IsRequired();

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

            // Same-module FK -> MiscMaster (no inverse navigation on the shared master)
            builder.HasOne(t => t.StructureStatus)
                .WithMany()
                .HasForeignKey(t => t.StructureStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // One structure per Company x Division (active rows)
            builder.HasIndex(t => new { t.CompanyId, t.DivisionId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(t => t.CompanyId);
            builder.HasIndex(t => t.DivisionId);
            builder.HasIndex(t => t.StructureStatusId);
        }
    }
}
