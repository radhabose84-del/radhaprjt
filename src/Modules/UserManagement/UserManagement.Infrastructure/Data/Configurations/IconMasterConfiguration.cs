using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class IconMasterConfiguration : IEntityTypeConfiguration<IconMaster>
    {
        public void Configure(EntityTypeBuilder<IconMaster> builder)
        {
            var isActiveConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeletedConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            builder.ToTable("IconMaster", "AppData");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Keyword)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(t => t.IconName)
                .IsRequired()
                .HasColumnType("varchar(100)");

            builder.Property(t => t.IconLibrary)
                .IsRequired()
                .HasColumnType("varchar(20)");

            builder.Property(t => t.Size)
                .IsRequired()
                .HasColumnType("int");

            // Stored as a JSON string; surfaced as a nested object on read
            builder.Property(t => t.Style)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            builder.Property(u => u.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(isActiveConverter)
                .IsRequired();

            builder.Property(u => u.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeletedConverter)
                .IsRequired();

            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(255)");

            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(255)");

            // Unique only among live rows — a soft-deleted keyword can be created again
            builder.HasIndex(t => t.Keyword).IsUnique().HasFilter("[IsDeleted] = 0");
        }
    }
}
