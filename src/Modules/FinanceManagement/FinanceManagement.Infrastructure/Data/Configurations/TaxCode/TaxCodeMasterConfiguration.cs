using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class TaxCodeMasterConfiguration : IEntityTypeConfiguration<TaxCodeMaster>
    {
        public void Configure(EntityTypeBuilder<TaxCodeMaster> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            // TaxType / TaxComponent / Direction are now MiscMaster-backed FKs (no string CHECK enums).
            builder.ToTable("TaxCodeMaster", "Finance", t =>
            {
                t.HasCheckConstraint("CK_TCM_Threshold",
                    "[ThresholdAmount] IS NULL OR [ThresholdAmount] >= 0");
                t.HasCheckConstraint("CK_TCM_ThresholdAgg",
                    "[ThresholdAggregate] IS NULL OR [ThresholdAggregate] >= 0");
            });

            builder.HasKey(t => t.Id);

            // No soft delete on this entity — "remove" = IsActive = Inactive.
            builder.Ignore(b => b.IsDeleted);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.CompanyId)
                .HasColumnName("CompanyId").HasColumnType("int").IsRequired();

            builder.Property(t => t.TaxCode)
                .HasColumnName("TaxCode").HasColumnType("varchar(20)").IsRequired();

            builder.Property(t => t.TaxName)
                .HasColumnName("TaxName").HasColumnType("varchar(150)").IsRequired();

            builder.Property(t => t.TaxTypeId)
                .HasColumnName("TaxTypeId").HasColumnType("int").IsRequired();

            builder.Property(t => t.TaxComponentId)
                .HasColumnName("TaxComponentId").HasColumnType("int").IsRequired(false);

            builder.Property(t => t.DirectionId)
                .HasColumnName("DirectionId").HasColumnType("int").IsRequired(false);

            builder.Property(t => t.ParentTaxCodeId)
                .HasColumnName("ParentTaxCodeId").HasColumnType("int").IsRequired(false);

            builder.Property(t => t.StatutorySection)
                .HasColumnName("StatutorySection").HasColumnType("varchar(20)").IsRequired(false);

            builder.Property(t => t.ThresholdAmount)
                .HasColumnName("ThresholdAmount").HasColumnType("decimal(18,2)").IsRequired(false);

            builder.Property(t => t.ThresholdAggregate)
                .HasColumnName("ThresholdAggregate").HasColumnType("decimal(18,2)").IsRequired(false);

            builder.Property(t => t.HsnSacCode)
                .HasColumnName("HsnSacCode").HasColumnType("varchar(10)").IsRequired(false);

            builder.Property(t => t.IsSystemOnlyPosting)
                .HasColumnName("IsSystemOnlyPosting").HasColumnType("bit")
                .HasDefaultValue(false).IsRequired();

            builder.Property(t => t.IsEefcRelevant)
                .HasColumnName("IsEefcRelevant").HasColumnType("bit")
                .HasDefaultValue(false).IsRequired();

            builder.Property(t => t.IsStatutoryFixed)
                .HasColumnName("IsStatutoryFixed").HasColumnType("bit")
                .HasDefaultValue(false).IsRequired();

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

            builder.HasIndex(t => new { t.CompanyId, t.TaxCode })
                .IsUnique()
                .HasDatabaseName("UX_TaxCodeMaster_Company_Code");
            builder.HasIndex(t => t.CompanyId).HasDatabaseName("IX_TaxCodeMaster_CompanyId");
            builder.HasIndex(t => t.TaxTypeId).HasDatabaseName("IX_TaxCodeMaster_TaxTypeId");
            builder.HasIndex(t => t.TaxComponentId).HasDatabaseName("IX_TaxCodeMaster_TaxComponentId");
            builder.HasIndex(t => t.DirectionId).HasDatabaseName("IX_TaxCodeMaster_DirectionId");
            builder.HasIndex(t => t.ParentTaxCodeId).HasDatabaseName("IX_TaxCodeMaster_ParentTaxCodeId");

            // Self-referencing parent/child (combined header -> CGST/SGST component children)
            builder.HasOne(t => t.ParentTaxCode)
                .WithMany(p => p.ComponentCodes)
                .HasForeignKey(t => t.ParentTaxCodeId)
                .OnDelete(DeleteBehavior.Restrict);

            // MiscMaster lookups (TAX TYPE / TAX COMPONENT / TAX DIRECTION)
            builder.HasOne(t => t.TaxTypeMaster)
                .WithMany()
                .HasForeignKey(t => t.TaxTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.TaxComponentMaster)
                .WithMany()
                .HasForeignKey(t => t.TaxComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.DirectionMaster)
                .WithMany()
                .HasForeignKey(t => t.DirectionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
