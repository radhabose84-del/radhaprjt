using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations
{
    public class GstrSectionAccountLinkageConfiguration : IEntityTypeConfiguration<GstrSectionAccountLinkage>
    {
        public void Configure(EntityTypeBuilder<GstrSectionAccountLinkage> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            builder.ToTable("GstrSectionAccountLinkage", "Finance", t =>
            {
                t.HasCheckConstraint("CK_GSAL_Tolerance", "[TolerancePercent] >= 0 AND [TolerancePercent] <= 100");
            });
            builder.HasKey(t => t.Id);

            builder.Ignore(b => b.IsDeleted);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.SectionMasterId).HasColumnName("SectionMasterId").HasColumnType("int").IsRequired();
            builder.Property(t => t.AccountRangeFrom).HasColumnName("AccountRangeFrom").HasColumnType("varchar(20)").IsRequired();
            builder.Property(t => t.AccountRangeTo).HasColumnName("AccountRangeTo").HasColumnType("varchar(20)").IsRequired();
            builder.Property(t => t.DerivedValue).HasColumnName("DerivedValue").HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(t => t.ExpectedValue).HasColumnName("ExpectedValue").HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.TolerancePercent).HasColumnName("TolerancePercent").HasColumnType("decimal(5,2)").HasDefaultValue(1m).IsRequired();

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

            builder.HasIndex(t => t.SectionMasterId).HasDatabaseName("IX_GstrSectionAccountLinkage_SectionMasterId");

            builder.HasOne(t => t.SectionMaster)
                .WithMany(s => s.AccountLinkages)
                .HasForeignKey(t => t.SectionMasterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
