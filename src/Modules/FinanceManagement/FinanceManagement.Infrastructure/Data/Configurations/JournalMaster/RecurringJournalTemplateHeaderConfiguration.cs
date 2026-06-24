using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations.JournalMaster
{
    public class RecurringJournalTemplateHeaderConfiguration : IEntityTypeConfiguration<RecurringJournalTemplateHeader>
    {
        public void Configure(EntityTypeBuilder<RecurringJournalTemplateHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            builder.ToTable("RecurringJournalTemplateHeader", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.TemplateName).HasColumnName("TemplateName").HasColumnType("varchar(150)").IsRequired();
            builder.Property(t => t.VoucherTypeId).HasColumnName("VoucherTypeId").HasColumnType("int").IsRequired();
            builder.Property(t => t.FrequencyId).HasColumnName("FrequencyId").HasColumnType("int").IsRequired();
            builder.Property(t => t.StartDate).HasColumnName("StartDate").HasColumnType("date").IsRequired();
            builder.Property(t => t.EndDate).HasColumnName("EndDate").HasColumnType("date").IsRequired(false);
            builder.Property(t => t.AutoPost).HasColumnName("AutoPost").HasColumnType("bit").HasDefaultValue(false).IsRequired();
            builder.Property(t => t.AmountAdjustmentRuleId).HasColumnName("AmountAdjustmentRuleId").HasColumnType("int").IsRequired();
            builder.Property(t => t.LowRisk).HasColumnName("LowRisk").HasColumnType("bit").HasDefaultValue(false).IsRequired();

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

            builder.HasIndex(t => t.TemplateName);

            builder.HasOne(t => t.VoucherType)
                .WithMany()
                .HasForeignKey(t => t.VoucherTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Frequency)
                .WithMany()
                .HasForeignKey(t => t.FrequencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.AmountAdjustmentRule)
                .WithMany()
                .HasForeignKey(t => t.AmountAdjustmentRuleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
