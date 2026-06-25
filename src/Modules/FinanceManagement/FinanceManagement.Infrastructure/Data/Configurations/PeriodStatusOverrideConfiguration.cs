using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class PeriodStatusOverrideConfiguration : IEntityTypeConfiguration<PeriodStatusOverride>
    {
        public void Configure(EntityTypeBuilder<PeriodStatusOverride> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("PeriodStatusOverride", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.FinancialPeriodId).HasColumnName("FinancialPeriodId").HasColumnType("int").IsRequired();
            builder.Property(t => t.CompanyId).HasColumnName("CompanyId").HasColumnType("int").IsRequired();

            builder.Property(t => t.FromStatusId).HasColumnName("FromStatusId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ToStatusId).HasColumnName("ToStatusId").HasColumnType("int").IsRequired();

            builder.Property(t => t.RequestedBy).HasColumnName("RequestedBy").HasColumnType("int").IsRequired();
            builder.Property(t => t.RequestedAt).HasColumnName("RequestedAt").IsRequired();
            builder.Property(t => t.RequestedReason)
                .HasColumnName("RequestedReason")
                .HasColumnType("varchar(500)")
                .IsRequired();

            builder.Property(t => t.CfoApproverId).HasColumnName("CfoApproverId").HasColumnType("int");
            builder.Property(t => t.CfoApprovedAt).HasColumnName("CfoApprovedAt");
            builder.Property(t => t.SysAdminApproverId).HasColumnName("SysAdminApproverId").HasColumnType("int");
            builder.Property(t => t.SysAdminApprovedAt).HasColumnName("SysAdminApprovedAt");

            builder.Property(t => t.OverrideStatusId).HasColumnName("OverrideStatusId").HasColumnType("int").IsRequired();
            builder.Property(t => t.AppliedAt).HasColumnName("AppliedAt");
            builder.Property(t => t.RejectionReason).HasColumnName("RejectionReason").HasColumnType("varchar(500)");

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

            builder.HasIndex(t => t.FinancialPeriodId);
            builder.HasIndex(t => t.OverrideStatusId);
            builder.HasIndex(t => t.CompanyId);

            // FinancialPeriodMaster (Restrict — keep override history even if period soft-deleted)
            builder.HasOne(t => t.FinancialPeriod)
                .WithMany(fp => fp.Overrides)
                .HasForeignKey(t => t.FinancialPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            // Three FKs to MiscMaster — each binds to its own reverse collection
            builder.HasOne(t => t.FromStatusMaster)
                .WithMany(mm => mm.PeriodStatusOverridesAsFrom)
                .HasForeignKey(t => t.FromStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ToStatusMaster)
                .WithMany(mm => mm.PeriodStatusOverridesAsTo)
                .HasForeignKey(t => t.ToStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.OverrideStatusMaster)
                .WithMany(mm => mm.PeriodStatusOverridesAsOverrideState)
                .HasForeignKey(t => t.OverrideStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
