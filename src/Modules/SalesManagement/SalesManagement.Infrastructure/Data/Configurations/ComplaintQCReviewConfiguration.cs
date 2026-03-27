using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class ComplaintQCReviewConfiguration : IEntityTypeConfiguration<ComplaintQCReview>
    {
        public void Configure(EntityTypeBuilder<ComplaintQCReview> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v)
            );

            builder.ToTable("ComplaintQCReview", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.ComplaintHeaderId).HasColumnName("ComplaintHeaderId").HasColumnType("int").IsRequired();
            builder.Property(t => t.PhysicalVerificationId).HasColumnName("PhysicalVerificationId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ComplaintStatusId).HasColumnName("ComplaintStatusId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.SeverityId).HasColumnName("SeverityId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.CompensationStructureId).HasColumnName("CompensationStructureId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.LabVerificationRequired).HasColumnName("LabVerificationRequired").HasColumnType("bit").IsRequired();
            builder.Property(t => t.LabResponsiblePersonId).HasColumnName("LabResponsiblePersonId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.ExpectedResolutionDate).HasColumnName("ExpectedResolutionDate").HasColumnType("date").HasConversion(dateOnlyConverter).IsRequired(false);
            builder.Property(t => t.Comments).HasColumnName("Comments").HasColumnType("nvarchar(1000)").IsRequired(false);

            // QC Review Audit Fields
            builder.Property(t => t.ReviewedBy).HasColumnName("ReviewedBy").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.ReviewedDate).HasColumnName("ReviewedDate").IsRequired(false);
            builder.Property(t => t.DecisionTimestamp).HasColumnName("DecisionTimestamp").IsRequired(false);

            builder.Property(b => b.IsActive).HasColumnName("IsActive").HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            builder.Property(b => b.IsDeleted).HasColumnName("IsDeleted").HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
            builder.Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasColumnType("int");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.CreatedByName).HasColumnName("CreatedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnName("CreatedIP").HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy").HasColumnType("int");
            builder.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
            builder.Property(t => t.ModifiedByName).HasColumnName("ModifiedByName").HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnName("ModifiedIP").HasColumnType("varchar(50)");

            // Same-module FK → ComplaintHeader (one review per complaint)
            builder.HasOne(t => t.ComplaintHeader)
                .WithMany()
                .HasForeignKey(t => t.ComplaintHeaderId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK → MiscMaster (PhysicalVerification)
            builder.HasOne(t => t.PhysicalVerification)
                .WithMany()
                .HasForeignKey(t => t.PhysicalVerificationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK → MiscMaster (QCComplaintStatus)
            builder.HasOne(t => t.ComplaintStatus)
                .WithMany()
                .HasForeignKey(t => t.ComplaintStatusId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK → MiscMaster (Severity)
            builder.HasOne(t => t.Severity)
                .WithMany()
                .HasForeignKey(t => t.SeverityId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK → MiscMaster (CompensationStructure)
            builder.HasOne(t => t.CompensationStructure)
                .WithMany()
                .HasForeignKey(t => t.CompensationStructureId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Child collection
            builder.HasMany(t => t.Assignments)
                .WithOne(a => a.ComplaintQCReview)
                .HasForeignKey(a => a.ComplaintQCReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => t.ComplaintHeaderId).IsUnique();
            builder.HasIndex(t => t.PhysicalVerificationId);
            builder.HasIndex(t => t.ComplaintStatusId);
            builder.HasIndex(t => t.SeverityId);
        }
    }
}
