using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PurchaseManagement.Domain.Entities.VendorEvaluation;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Data.Configurations.VendorEvaluation
{
    public class VendorEvaluationHeaderConfiguration : IEntityTypeConfiguration<VendorEvaluationHeader>
    {
        public void Configure(EntityTypeBuilder<VendorEvaluationHeader> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("VendorEvaluationHeader", "Purchase");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.EvaluationCode)
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(t => t.VendorId)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.EvaluationMonth)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.EvaluationYear)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.EvaluationDate)
                .IsRequired();

            builder.Property(t => t.TotalWeightedScore)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(t => t.GradeId)
                .HasColumnType("int");

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
            builder.HasIndex(t => t.EvaluationCode).IsUnique();
            builder.HasIndex(t => t.VendorId);
            builder.HasIndex(t => new { t.VendorId, t.EvaluationMonth, t.EvaluationYear }).IsUnique();

            // Foreign key — same-module VendorRatingGrade (nullable)
            builder.HasOne(t => t.Grade)
                .WithMany(g => g.VendorEvaluationHeaders)
                .HasForeignKey(t => t.GradeId)
                .OnDelete(DeleteBehavior.Restrict);

            // No FK constraint for VendorId — cross-module (PartyManagement)
        }
    }
}
