using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PurchaseManagement.Domain.Entities.VendorEvaluation;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.VendorEvaluation
{
    public class DeliveryScoreRuleConfiguration : IEntityTypeConfiguration<DeliveryScoreRule>
    {
        public void Configure(EntityTypeBuilder<DeliveryScoreRule> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("DeliveryScoreRule", "Purchase");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.RuleCode)
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnType("varchar(200)")
                .IsRequired();

            builder.Property(t => t.DelayDaysFrom)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.DelayDaysTo)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Score)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.SortOrder)
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(20)");

            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(20)");

            // Indexes
            builder.HasIndex(t => t.RuleCode).IsUnique();
        }
    }
}
