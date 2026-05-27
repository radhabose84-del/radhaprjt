using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PurchaseManagement.Domain.Entities.VendorEvaluation;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.VendorEvaluation
{
    public class VendorEvaluationCriteriaConfiguration : IEntityTypeConfiguration<VendorEvaluationCriteria>
    {
        public void Configure(EntityTypeBuilder<VendorEvaluationCriteria> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("VendorEvaluationCriteria", "Purchase");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.CriteriaCode)
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.CriteriaName)
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasColumnType("varchar(500)");

            builder.Property(t => t.WeightagePercent)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.ScoringMethodId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.MinimumScore)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.RatingImpactId)
                .HasColumnType("int")
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
            builder.HasIndex(t => t.CriteriaCode).IsUnique();

            // Foreign keys — same-module MiscMaster
            builder.HasOne(t => t.ScoringMethod)
                .WithMany(m => m.VendorCriteriaScoringMethod)
                .HasForeignKey(t => t.ScoringMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.RatingImpact)
                .WithMany(m => m.VendorCriteriaRatingImpact)
                .HasForeignKey(t => t.RatingImpactId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
