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

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("TaxCodeMaster", "Finance", t =>
            {
                t.HasCheckConstraint("CK_TCM_TaxType",
                    "[TaxType] IN ('GST_IN','GST_OUT','IGST','TDS','CUSTOMS')");
                t.HasCheckConstraint("CK_TCM_Component",
                    "[TaxComponent] IN ('COMBINED','CGST','SGST','IGST','CESS','NA')");
                t.HasCheckConstraint("CK_TCM_Direction",
                    "[Direction] IS NULL OR [Direction] IN ('INPUT','OUTPUT')");
                t.HasCheckConstraint("CK_TCM_Threshold",
                    "[ThresholdAmount] IS NULL OR [ThresholdAmount] >= 0");
                t.HasCheckConstraint("CK_TCM_ThresholdAgg",
                    "[ThresholdAggregate] IS NULL OR [ThresholdAggregate] >= 0");
                t.HasCheckConstraint("CK_TCM_ParentLink",
                    "([TaxComponent] IN ('CGST','SGST','IGST','CESS') AND [ParentTaxCodeId] IS NOT NULL) "
                    + "OR ([TaxComponent] IN ('COMBINED','NA') AND [ParentTaxCodeId] IS NULL)");
            });

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.CompanyId)
                .HasColumnName("CompanyId").HasColumnType("int").IsRequired();

            builder.Property(t => t.TaxCode)
                .HasColumnName("TaxCode").HasColumnType("varchar(20)").IsRequired();

            builder.Property(t => t.TaxName)
                .HasColumnName("TaxName").HasColumnType("varchar(150)").IsRequired();

            builder.Property(t => t.TaxType)
                .HasColumnName("TaxType").HasColumnType("varchar(15)").IsRequired();

            builder.Property(t => t.TaxComponent)
                .HasColumnName("TaxComponent").HasColumnType("varchar(10)")
                .HasDefaultValue("COMBINED").IsRequired();

            builder.Property(t => t.ParentTaxCodeId)
                .HasColumnName("ParentTaxCodeId").HasColumnType("int").IsRequired(false);

            builder.Property(t => t.Direction)
                .HasColumnName("Direction").HasColumnType("varchar(10)").IsRequired(false);

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

            builder.HasIndex(t => new { t.CompanyId, t.TaxCode })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("UX_TaxCodeMaster_Company_Code");
            builder.HasIndex(t => t.CompanyId).HasDatabaseName("IX_TaxCodeMaster_CompanyId");
            builder.HasIndex(t => t.TaxType).HasDatabaseName("IX_TaxCodeMaster_TaxType");
            builder.HasIndex(t => t.ParentTaxCodeId).HasDatabaseName("IX_TaxCodeMaster_ParentTaxCodeId");

            // Self-referencing parent/child (combined header -> CGST/SGST component children)
            builder.HasOne(t => t.ParentTaxCode)
                .WithMany(p => p.ComponentCodes)
                .HasForeignKey(t => t.ParentTaxCodeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
