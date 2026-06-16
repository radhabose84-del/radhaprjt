using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class TaxCodeRateVersionConfiguration : IEntityTypeConfiguration<TaxCodeRateVersion>
    {
        public void Configure(EntityTypeBuilder<TaxCodeRateVersion> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            builder.ToTable("TaxCodeRateVersion", "Finance", t =>
            {
                t.HasCheckConstraint("CK_TCRV_Rate",
                    "[RatePercent] >= 0 AND [RatePercent] <= 100");
                t.HasCheckConstraint("CK_TCRV_Dates",
                    "[EffectiveTo] IS NULL OR [EffectiveTo] > [EffectiveFrom]");
            });

            builder.HasKey(t => t.Id);

            // No soft delete on this entity — versions are closed via EffectiveTo, never deleted.
            builder.Ignore(b => b.IsDeleted);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();

            builder.Property(t => t.TaxCodeId)
                .HasColumnName("TaxCodeId").HasColumnType("int").IsRequired();

            builder.Property(t => t.VersionNo)
                .HasColumnName("VersionNo").HasColumnType("int").IsRequired();

            builder.Property(t => t.RatePercent)
                .HasColumnName("RatePercent").HasColumnType("decimal(7,4)").IsRequired();

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

            builder.HasIndex(t => new { t.TaxCodeId, t.VersionNo })
                .IsUnique()
                .HasDatabaseName("UQ_TCRV_Version");

            // Exactly one OPEN version per code => "prior retained, new applies forward"
            builder.HasIndex(t => t.TaxCodeId)
                .IsUnique()
                .HasFilter("[EffectiveTo] IS NULL")
                .HasDatabaseName("UX_TaxCodeRateVersion_OpenPerCode");

            builder.HasIndex(t => new { t.TaxCodeId, t.EffectiveFrom })
                .HasDatabaseName("IX_TaxCodeRateVersion_Code_From");

            builder.HasOne(t => t.TaxCode)
                .WithMany(c => c.RateVersions)
                .HasForeignKey(t => t.TaxCodeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
