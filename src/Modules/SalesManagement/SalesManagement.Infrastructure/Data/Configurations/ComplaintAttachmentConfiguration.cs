using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Data.Configurations
{
    public class ComplaintAttachmentConfiguration : IEntityTypeConfiguration<ComplaintAttachment>
    {
        public void Configure(EntityTypeBuilder<ComplaintAttachment> builder)
        {
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("ComplaintAttachment", "Sales");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.ComplaintHeaderId).HasColumnType("int").IsRequired();
            builder.Property(t => t.FileName).HasColumnType("nvarchar(255)").IsRequired(false);
            builder.Property(t => t.FilePath).HasColumnType("nvarchar(500)").IsRequired(false);
            builder.Property(t => t.FileType).HasColumnType("nvarchar(50)").IsRequired(false);
            builder.Property(t => t.FileSize).HasColumnType("bigint").IsRequired(false);

            builder.Property(b => b.IsActive).HasColumnType("bit").HasConversion(statusConverter).IsRequired();
            builder.Property(b => b.IsDeleted).HasColumnType("bit").HasConversion(isDeleteConverter).IsRequired();
            builder.Property(t => t.CreatedBy).HasColumnType("int");
            builder.Property(t => t.CreatedDate);
            builder.Property(t => t.CreatedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.CreatedIP).HasColumnType("varchar(50)");
            builder.Property(t => t.ModifiedBy).HasColumnType("int");
            builder.Property(t => t.ModifiedDate);
            builder.Property(t => t.ModifiedByName).HasColumnType("varchar(100)");
            builder.Property(t => t.ModifiedIP).HasColumnType("varchar(50)");

            builder.HasIndex(t => t.ComplaintHeaderId);

            builder.HasOne(t => t.ComplaintHeader)
                .WithMany()
                .HasForeignKey(t => t.ComplaintHeaderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
