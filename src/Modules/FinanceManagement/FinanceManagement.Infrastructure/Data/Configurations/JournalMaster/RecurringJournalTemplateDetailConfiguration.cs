using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Data.Configurations.JournalMaster
{
    public class RecurringJournalTemplateDetailConfiguration : IEntityTypeConfiguration<RecurringJournalTemplateDetail>
    {
        public void Configure(EntityTypeBuilder<RecurringJournalTemplateDetail> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            builder.ToTable("RecurringJournalTemplateDetail", "Finance");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.TemplateId).HasColumnName("TemplateId").HasColumnType("int").IsRequired();
            builder.Property(t => t.LineNo).HasColumnName("LineNo").HasColumnType("int").IsRequired();
            builder.Property(t => t.GlAccountId).HasColumnName("GlAccountId").HasColumnType("int").IsRequired();
            builder.Property(t => t.DrAmount).HasColumnName("DrAmount").HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(t => t.CrAmount).HasColumnName("CrAmount").HasColumnType("decimal(18,2)").IsRequired(false);
            builder.Property(t => t.AmountFormula).HasColumnName("AmountFormula").HasColumnType("varchar(300)").IsRequired(false);
            builder.Property(t => t.CostCentreId).HasColumnName("CostCentreId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.ProfitCentreId).HasColumnName("ProfitCentreId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.LineNarration).HasColumnName("LineNarration").HasColumnType("varchar(500)").IsRequired(false);

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

            builder.HasIndex(t => t.TemplateId);

            builder.HasOne(t => t.Template)
                .WithMany(p => p.Lines)
                .HasForeignKey(t => t.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.GlAccount)
                .WithMany()
                .HasForeignKey(t => t.GlAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.CostCentre)
                .WithMany()
                .HasForeignKey(t => t.CostCentreId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ProfitCentre)
                .WithMany()
                .HasForeignKey(t => t.ProfitCentreId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
