using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PurchaseManagement.Domain.Entities.VendorEvaluation;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.VendorEvaluation
{
    public class VendorEvaluationDetailConfiguration : IEntityTypeConfiguration<VendorEvaluationDetail>
    {
        public void Configure(EntityTypeBuilder<VendorEvaluationDetail> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("VendorEvaluationDetail", "Purchase");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.VendorEvaluationHeaderId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.CriteriaId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.Score)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.WeightagePercent)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.WeightedScore)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.ScoringMethod)
                .HasColumnType("varchar(20)");

            builder.Property(t => t.Remarks)
                .HasColumnType("varchar(500)");

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
            builder.HasIndex(t => t.VendorEvaluationHeaderId);
            builder.HasIndex(t => t.CriteriaId);

            // Foreign key — same-module VendorEvaluationHeader
            builder.HasOne(t => t.VendorEvaluationHeader)
                .WithMany(h => h.VendorEvaluationDetails)
                .HasForeignKey(t => t.VendorEvaluationHeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Foreign key — same-module VendorEvaluationCriteria
            builder.HasOne(t => t.VendorEvaluationCriteria)
                .WithMany(c => c.VendorEvaluationDetails)
                .HasForeignKey(t => t.CriteriaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
