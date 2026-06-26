using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations.JournalMaster
{
    public class JournalHeaderConfiguration : IEntityTypeConfiguration<JournalHeader>
    {
        public void Configure(EntityTypeBuilder<JournalHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            // US-GL01-10 immutability triggers exist on this table — declare them so EF Core emits
            // trigger-compatible DML (no OUTPUT clause without INTO), per SQL Server's restriction.
            builder.ToTable("JournalHeader", "Finance", t => t.HasTrigger("TR_JournalHeader_Immutable"));
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.CompanyId).HasColumnName("CompanyId").HasColumnType("int").IsRequired();
            builder.Property(t => t.UnitId).HasColumnName("UnitId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.VoucherTypeId).HasColumnName("VoucherTypeId").HasColumnType("int").IsRequired();
            builder.Property(t => t.VoucherNo).HasColumnName("VoucherNo").HasColumnType("varchar(30)").IsRequired(false);
            builder.Property(t => t.VoucherDate).HasColumnName("VoucherDate").HasColumnType("date").IsRequired();
            builder.Property(t => t.PostingDate).HasColumnName("PostingDate").HasColumnType("date").IsRequired(false);
            builder.Property(t => t.FinancialYearId).HasColumnName("FinancialYearId").HasColumnType("int").IsRequired();
            builder.Property(t => t.AccountingPeriodId).HasColumnName("AccountingPeriodId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.Narration).HasColumnName("Narration").HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(t => t.StatusId).HasColumnName("StatusId").HasColumnType("int").IsRequired();
            builder.Property(t => t.SourceId).HasColumnName("SourceId").HasColumnType("int").IsRequired();
            builder.Property(t => t.TriggerDocType).HasColumnName("TriggerDocType").HasColumnType("varchar(30)").IsRequired(false);
            builder.Property(t => t.TriggerDocRef).HasColumnName("TriggerDocRef").HasColumnType("varchar(50)").IsRequired(false);
            builder.Property(t => t.AutoApproved).HasColumnName("AutoApproved").HasColumnType("bit").HasDefaultValue(false).IsRequired();
            builder.Property(t => t.TotalDr).HasColumnName("TotalDr").HasColumnType("decimal(18,2)").HasDefaultValue(0m).IsRequired();
            builder.Property(t => t.TotalCr).HasColumnName("TotalCr").HasColumnType("decimal(18,2)").HasDefaultValue(0m).IsRequired();
            builder.Property(t => t.ReversalOfId).HasColumnName("ReversalOfId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.IsReversal).HasColumnName("IsReversal").HasColumnType("bit").HasDefaultValue(false).IsRequired();
            builder.Property(t => t.CopiedFromRef).HasColumnName("CopiedFromRef").HasColumnType("varchar(30)").IsRequired(false);
            builder.Property(t => t.ImportBatchId).HasColumnName("ImportBatchId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.CleanupAlertedAt).HasColumnName("CleanupAlertedAt").IsRequired(false);
            builder.Property(t => t.ApprovedBy).HasColumnName("ApprovedBy").HasColumnType("varchar(100)").IsRequired(false);
            builder.Property(t => t.ApprovedAt).HasColumnName("ApprovedAt").IsRequired(false);
            builder.Property(t => t.RejectedBy).HasColumnName("RejectedBy").HasColumnType("varchar(100)").IsRequired(false);
            builder.Property(t => t.RejectedAt).HasColumnName("RejectedAt").IsRequired(false);
            builder.Property(t => t.RejectReason).HasColumnName("RejectReason").HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(t => t.PostedBy).HasColumnName("PostedBy").HasColumnType("varchar(100)").IsRequired(false);
            builder.Property(t => t.PostedAt).HasColumnName("PostedAt").IsRequired(false);

            // US-GL03-04 — backdating audit columns.
            // IsBackdated is a PERSISTED computed column so the late-posting report and the weekly
            // CFO digest can drive index seeks against a filtered nonclustered index.
            builder.Property(t => t.IsBackdated)
                .HasColumnName("IsBackdated")
                .HasComputedColumnSql(
                    "CASE WHEN VoucherDate IS NULL OR PostedAt IS NULL THEN CAST(0 AS BIT) " +
                    "WHEN VoucherDate < CAST(PostedAt AS DATE) THEN CAST(1 AS BIT) " +
                    "ELSE CAST(0 AS BIT) END",
                    stored: true);

            builder.Property(t => t.BackdateReason)
                .HasColumnName("BackdateReason")
                .HasColumnType("varchar(500)")
                .IsRequired(false);

            builder.Property(t => t.BackdateAcknowledgedBy)
                .HasColumnName("BackdateAcknowledgedBy")
                .HasColumnType("int")
                .IsRequired(false);

            builder.Property(t => t.BackdateAcknowledgedAt)
                .HasColumnName("BackdateAcknowledgedAt")
                .IsRequired(false);

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

            // VoucherNo unique only once assigned (drafts have NULL)
            builder.HasIndex(t => t.VoucherNo)
                .IsUnique()
                .HasFilter("[VoucherNo] IS NOT NULL")
                .HasDatabaseName("UX_JournalHeader_VoucherNo");

            builder.HasIndex(t => new { t.CompanyId, t.VoucherDate });

            // US-GL03-04 — drives the late-posting report + weekly CFO digest scans. Filtered indexes
            // cannot reference computed columns in SQL Server (even persisted ones), so we keep a
            // composite (IsBackdated, IsDeleted, CompanyId) index instead — a WHERE IsBackdated = 1
            // AND IsDeleted = 0 still seeks the leading key, then narrows by company.
            builder.HasIndex(t => new { t.IsBackdated, t.IsDeleted, t.CompanyId })
                .HasDatabaseName("IX_JournalHeader_IsBackdated");
            builder.HasIndex(t => t.StatusId);
            builder.HasIndex(t => t.SourceId);
            builder.HasIndex(t => t.AccountingPeriodId);
            builder.HasIndex(t => t.ReversalOfId);
            builder.HasIndex(t => t.CreatedBy);

            builder.HasOne(t => t.VoucherType)
                .WithMany()
                .HasForeignKey(t => t.VoucherTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.AccountingPeriod)
                .WithMany()
                .HasForeignKey(t => t.AccountingPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.JournalStatus)
                .WithMany()
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Source)
                .WithMany()
                .HasForeignKey(t => t.SourceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ReversalOf)
                .WithMany()
                .HasForeignKey(t => t.ReversalOfId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ImportBatch)
                .WithMany()
                .HasForeignKey(t => t.ImportBatchId)
                .OnDelete(DeleteBehavior.Restrict);

            // CompanyId / UnitId / FinancialYearId / user ids are cross-module — no DB FK constraint.
        }
    }
}
