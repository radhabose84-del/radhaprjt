using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations.JournalMaster
{
    public class AccountingPeriodConfiguration : IEntityTypeConfiguration<AccountingPeriod>
    {
        public void Configure(EntityTypeBuilder<AccountingPeriod> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            builder.ToTable("AccountingPeriod", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.CompanyId).HasColumnName("CompanyId").HasColumnType("int").IsRequired();
            builder.Property(t => t.FinancialYearId).HasColumnName("FinancialYearId").HasColumnType("int").IsRequired();
            builder.Property(t => t.PeriodName).HasColumnName("PeriodName").HasColumnType("varchar(20)").IsRequired();
            builder.Property(t => t.PeriodNo).HasColumnName("PeriodNo").HasColumnType("int").IsRequired();
            builder.Property(t => t.StartDate).HasColumnName("StartDate").HasColumnType("date").IsRequired();
            builder.Property(t => t.EndDate).HasColumnName("EndDate").HasColumnType("date").IsRequired();
            builder.Property(t => t.StatusId).HasColumnName("StatusId").HasColumnType("int").IsRequired();

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

            builder.HasIndex(t => new { t.CompanyId, t.FinancialYearId, t.PeriodNo })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(t => new { t.CompanyId, t.FinancialYearId });
            builder.HasIndex(t => new { t.StartDate, t.EndDate });
            builder.HasIndex(t => t.StatusId);

            // StatusId -> Finance.MiscMaster (PERIOD_STATUS)
            builder.HasOne(t => t.PeriodStatus)
                .WithMany()
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // CompanyId / FinancialYearId are cross-module references — no DB FK constraint.
        }
    }
}
