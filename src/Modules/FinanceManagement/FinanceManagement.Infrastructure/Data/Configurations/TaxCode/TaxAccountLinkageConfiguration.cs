using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class TaxAccountLinkageConfiguration : IEntityTypeConfiguration<TaxAccountLinkage>
    {
        public void Configure(EntityTypeBuilder<TaxAccountLinkage> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            builder.ToTable("TaxAccountLinkage", "Finance", t =>
            {
                t.HasCheckConstraint("CK_TAL_Dates",
                    "[EffectiveTo] IS NULL OR [EffectiveTo] > [EffectiveFrom]");
            });

            builder.HasKey(t => t.Id);

            // No soft delete on this entity — "remove" = IsActive = Inactive.
            builder.Ignore(b => b.IsDeleted);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.CompanyId)
                .HasColumnName("CompanyId").HasColumnType("int").IsRequired();

            builder.Property(t => t.TaxCodeId)
                .HasColumnName("TaxCodeId").HasColumnType("int").IsRequired();

            builder.Property(t => t.GlAccountId)
                .HasColumnName("GlAccountId").HasColumnType("int").IsRequired();

            builder.Property(t => t.ControlAccountId)
                .HasColumnName("ControlAccountId").HasColumnType("int").IsRequired(false);

            builder.Property(t => t.StatusId)
                .HasColumnName("StatusId").HasColumnType("int").IsRequired();

            builder.Property(t => t.EffectiveFrom)
                .HasColumnName("EffectiveFrom").HasColumnType("date").IsRequired();

            builder.Property(t => t.EffectiveTo)
                .HasColumnName("EffectiveTo").HasColumnType("date").IsRequired(false);

            builder.Property(t => t.ChangeReason)
                .HasColumnName("ChangeReason").HasColumnType("varchar(500)").IsRequired(false);

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive").HasColumnType("bit")
                .HasConversion(statusConverter).IsRequired();

            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // One ACTIVE linkage per GL account — filtered on the static IsActive bit
            // (StatusId is a dynamic MiscMaster id and cannot be used in a filtered index).
            builder.HasIndex(t => new { t.CompanyId, t.GlAccountId })
                .IsUnique()
                .HasFilter("[EffectiveTo] IS NULL AND [IsActive] = 1")
                .HasDatabaseName("UX_TaxAccountLinkage_ActivePerAccount");

            builder.HasIndex(t => t.TaxCodeId).HasDatabaseName("IX_TaxAccountLinkage_TaxCodeId");
            builder.HasIndex(t => t.GlAccountId).HasDatabaseName("IX_TaxAccountLinkage_GlAccountId");
            builder.HasIndex(t => t.ControlAccountId).HasDatabaseName("IX_TaxAccountLinkage_ControlAccountId");
            builder.HasIndex(t => t.StatusId).HasDatabaseName("IX_TaxAccountLinkage_StatusId");

            builder.HasOne(t => t.TaxCode)
                .WithMany(c => c.Linkages)
                .HasForeignKey(t => t.TaxCodeId)
                .OnDelete(DeleteBehavior.Restrict);

            // GlAccountMaster reverse collection not modelled (keeps GlAccountMaster untouched)
            builder.HasOne(t => t.GlAccount)
                .WithMany()
                .HasForeignKey(t => t.GlAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Control account type -> Finance.MiscMaster, nullable
            builder.HasOne(t => t.ControlAccount)
                .WithMany()
                .HasForeignKey(t => t.ControlAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Approval status -> Finance.MiscMaster (ApprovalStatus)
            builder.HasOne(t => t.StatusMaster)
                .WithMany()
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
