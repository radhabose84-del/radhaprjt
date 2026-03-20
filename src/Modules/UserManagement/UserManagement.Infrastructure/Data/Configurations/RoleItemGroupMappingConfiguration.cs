using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class RoleItemGroupMappingConfiguration : IEntityTypeConfiguration<RoleItemGroupMapping>
    {
        public void Configure(EntityTypeBuilder<RoleItemGroupMapping> builder)
        {
            var isActiveConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive
            );

            var isDeletedConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("RoleItemGroupMapping", "AppSecurity");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.RoleId)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(r => r.ItemGroupId)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(r => r.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(isActiveConverter)
                .IsRequired();

            builder.Property(r => r.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeletedConverter)
                .IsRequired();

            builder.Property(b => b.CreatedByName)
                .HasColumnType("varchar(100)");

            builder.Property(b => b.CreatedIP)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(100)");

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(50)");

            // Unique constraint: (RoleId, ItemGroupId)
            builder.HasIndex(r => new { r.RoleId, r.ItemGroupId }).IsUnique();

            // FK: RoleId -> UserRole (same-module)
            builder.HasOne(r => r.UserRole)
                .WithMany(ur => ur.RoleItemGroupMappings)
                .HasForeignKey(r => r.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ItemGroupId is a cross-module FK (InventoryManagement) — no DB constraint
        }
    }
}
