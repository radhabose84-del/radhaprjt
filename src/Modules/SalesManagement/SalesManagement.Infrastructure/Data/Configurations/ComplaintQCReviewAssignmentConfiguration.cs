using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class ComplaintQCReviewAssignmentConfiguration : IEntityTypeConfiguration<ComplaintQCReviewAssignment>
    {
        public void Configure(EntityTypeBuilder<ComplaintQCReviewAssignment> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("ComplaintQCReviewAssignment", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
            builder.Property(t => t.ComplaintQCReviewId).HasColumnName("ComplaintQCReviewId").HasColumnType("int").IsRequired();
            builder.Property(t => t.RoleId).HasColumnName("RoleId").HasColumnType("int").IsRequired();
            builder.Property(t => t.ResponsiblePersonId).HasColumnName("ResponsiblePersonId").HasColumnType("int").IsRequired();
            builder.Property(t => t.IsMandatory).HasColumnName("IsMandatory").HasColumnType("bit").IsRequired();
            builder.Property(t => t.AssignmentStatusId).HasColumnName("AssignmentStatusId").HasColumnType("int").IsRequired();

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

            // Same-module FK → MiscMaster (QCAssignmentRole)
            builder.HasOne(t => t.Role)
                .WithMany()
                .HasForeignKey(t => t.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Same-module FK → MiscMaster (QCAssignmentStatus)
            builder.HasOne(t => t.AssignmentStatus)
                .WithMany()
                .HasForeignKey(t => t.AssignmentStatusId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.ComplaintQCReviewId);
            builder.HasIndex(t => t.RoleId);
            builder.HasIndex(t => t.ResponsiblePersonId);
        }
    }
}
