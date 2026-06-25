using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class FinancialPeriodMasterConfiguration : IEntityTypeConfiguration<FinancialPeriodMaster>
    {
        public void Configure(EntityTypeBuilder<FinancialPeriodMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("FinancialPeriodMaster", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.FinancialYearId).HasColumnName("FinancialYearId").HasColumnType("int").IsRequired();
            builder.Property(t => t.CompanyId).HasColumnName("CompanyId").HasColumnType("int").IsRequired();

            builder.Property(t => t.PeriodNumber)
                .HasColumnName("PeriodNumber")
                .HasColumnType("tinyint")
                .IsRequired();

            builder.Property(t => t.PeriodName)
                .HasColumnName("PeriodName")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.StartDate).HasColumnName("StartDate").HasColumnType("date").IsRequired();
            builder.Property(t => t.EndDate).HasColumnName("EndDate").HasColumnType("date").IsRequired();

            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.IsAdjustmentPeriod)
                .HasColumnName("IsAdjustmentPeriod")
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .IsRequired();

            // US-GL03-02 — denormalised last-status-change audit pointers
            builder.Property(t => t.LastStatusChangedBy)
                .HasColumnName("LastStatusChangedBy")
                .HasColumnType("int");

            builder.Property(t => t.LastStatusChangedAt)
                .HasColumnName("LastStatusChangedAt");

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

            builder.HasIndex(t => new { t.FinancialYearId, t.PeriodNumber }).IsUnique();
            // Period 12 + Period 13 may share dates (year-end + adjustment) — include flag in key
            builder.HasIndex(t => new { t.CompanyId, t.StartDate, t.EndDate, t.IsAdjustmentPeriod }).IsUnique();
            builder.HasIndex(t => t.CompanyId);
            builder.HasIndex(t => new { t.StartDate, t.EndDate });

            // Same-module FK to FinancialYearMaster (cascade delete with header)
            builder.HasOne(t => t.FinancialYear)
                .WithMany(fy => fy.Periods)
                .HasForeignKey(t => t.FinancialYearId)
                .OnDelete(DeleteBehavior.Cascade);

            // Same-module FK to MiscMaster (Status — FPS)
            builder.HasOne(t => t.StatusMaster)
                .WithMany(mm => mm.FinancialPeriodsAsStatus)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
