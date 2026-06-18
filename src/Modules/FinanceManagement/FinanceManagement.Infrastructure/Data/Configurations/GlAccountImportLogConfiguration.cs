using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class GlAccountImportLogConfiguration : IEntityTypeConfiguration<GlAccountImportLog>
    {
        public void Configure(EntityTypeBuilder<GlAccountImportLog> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("GlAccountImportLog", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            // Cross-module (UserManagement.Company) — column + index only, no DB FK constraint.
            builder.Property(t => t.CompanyId).HasColumnName("CompanyId").HasColumnType("int").IsRequired();

            builder.Property(t => t.FileName).HasColumnName("FileName").HasColumnType("varchar(260)").IsRequired();
            builder.Property(t => t.FileFormat).HasColumnName("FileFormat").HasColumnType("varchar(10)").IsRequired();
            builder.Property(t => t.ImportMode).HasColumnName("ImportMode").HasColumnType("varchar(20)").IsRequired();

            builder.Property(t => t.TotalRows).HasColumnName("TotalRows").HasColumnType("int").IsRequired();
            builder.Property(t => t.GroupRows).HasColumnName("GroupRows").HasColumnType("int").IsRequired();
            builder.Property(t => t.AccountRows).HasColumnName("AccountRows").HasColumnType("int").IsRequired();
            builder.Property(t => t.ValidRows).HasColumnName("ValidRows").HasColumnType("int").IsRequired();
            builder.Property(t => t.InvalidRows).HasColumnName("InvalidRows").HasColumnType("int").IsRequired();
            builder.Property(t => t.ImportedGroups).HasColumnName("ImportedGroups").HasColumnType("int").IsRequired();
            builder.Property(t => t.ImportedAccounts).HasColumnName("ImportedAccounts").HasColumnType("int").IsRequired();
            builder.Property(t => t.SkippedRows).HasColumnName("SkippedRows").HasColumnType("int").IsRequired();

            builder.Property(t => t.Status).HasColumnName("Status").HasColumnType("varchar(30)").IsRequired();
            builder.Property(t => t.DurationMs).HasColumnName("DurationMs").HasColumnType("int").IsRequired();

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

            builder.HasIndex(t => t.CompanyId);
            builder.HasIndex(t => t.CreatedDate);

            // One log → many row errors. Restrict so a log cannot be hard-deleted while its
            // error report still exists (records are soft-deleted, never physically removed).
            builder.HasMany(t => t.Errors)
                .WithOne(e => e.ImportLog)
                .HasForeignKey(e => e.ImportLogId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
