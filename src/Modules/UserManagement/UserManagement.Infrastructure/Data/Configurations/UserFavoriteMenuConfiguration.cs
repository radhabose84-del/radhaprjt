using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class UserFavoriteMenuConfiguration : IEntityTypeConfiguration<UserFavoriteMenu>
    {
        public void Configure(EntityTypeBuilder<UserFavoriteMenu> builder)
        {
            var isActiveConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeletedConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("UserFavoriteMenu", "AppData");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.UserId)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(t => t.MenuId)
                .IsRequired()
                .HasColumnType("int");

            // Unique composite index — prevents duplicate favorites
            builder.HasIndex(t => new { t.UserId, t.MenuId })
                .IsUnique();

            // Index for fast user lookup
            builder.HasIndex(t => t.UserId);

            // Same-module FK to Menus (AppData schema)
            builder.HasOne(t => t.Menu)
                .WithMany()
                .HasForeignKey(t => t.MenuId)
                .OnDelete(DeleteBehavior.Cascade);

            // No DB FK to Users (different schema: AppSecurity)
            // UserId is validated via JWT token — always a valid logged-in user

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
        }
    }
}
