using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class ComplaintDepartmentFeedbackConfiguration : IEntityTypeConfiguration<ComplaintDepartmentFeedback>
    {
        public void Configure(EntityTypeBuilder<ComplaintDepartmentFeedback> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("ComplaintDepartmentFeedback", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.AssignmentId).HasColumnName("AssignmentId").HasColumnType("int").IsRequired();
            builder.Property(t => t.RootCauseText).HasColumnName("RootCauseText").HasColumnType("nvarchar(2000)").IsRequired(false);
            builder.Property(t => t.RootCauseCategoryId).HasColumnName("RootCauseCategoryId").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.CorrectiveAction).HasColumnName("CorrectiveAction").HasColumnType("nvarchar(2000)").IsRequired();
            builder.Property(t => t.PreventiveAction).HasColumnName("PreventiveAction").HasColumnType("nvarchar(2000)").IsRequired(false);
            builder.Property(t => t.Remarks).HasColumnName("Remarks").HasColumnType("nvarchar(1000)").IsRequired(false);
            builder.Property(t => t.FeedbackStatusId).HasColumnName("FeedbackStatusId").HasColumnType("int").IsRequired();
            builder.Property(t => t.SubmittedBy).HasColumnName("SubmittedBy").HasColumnType("int").IsRequired(false);
            builder.Property(t => t.SubmittedDate).HasColumnName("SubmittedDate").IsRequired(false);
            builder.Property(t => t.ReworkCount).HasColumnName("ReworkCount").HasColumnType("int").HasDefaultValue(0).IsRequired();
            builder.Property(t => t.ReworkReason).HasColumnName("ReworkReason").HasColumnType("nvarchar(1000)").IsRequired(false);

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

            // Same-module FK → ComplaintQCReviewAssignment (one feedback per assignment)
            builder.HasOne(t => t.Assignment)
                .WithMany()
                .HasForeignKey(t => t.AssignmentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK → MiscMaster (RootCauseCategory)
            builder.HasOne(t => t.RootCauseCategory)
                .WithMany()
                .HasForeignKey(t => t.RootCauseCategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK → MiscMaster (FeedbackStatus)
            builder.HasOne(t => t.FeedbackStatus)
                .WithMany()
                .HasForeignKey(t => t.FeedbackStatusId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Child collection
            builder.HasMany(t => t.Attachments)
                .WithOne(a => a.Feedback)
                .HasForeignKey(a => a.FeedbackId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => t.AssignmentId).IsUnique();
            builder.HasIndex(t => t.FeedbackStatusId);
            builder.HasIndex(t => t.RootCauseCategoryId);
            builder.HasIndex(t => t.SubmittedBy);
        }
    }
}
