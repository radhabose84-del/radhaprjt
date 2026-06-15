using UserManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class RoleMenuPrivilegesConfiguration : IEntityTypeConfiguration<RoleMenuPrivileges>
    {
        public void Configure(EntityTypeBuilder<RoleMenuPrivileges> builder)
        {
            var isActiveConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,
                v => v ? Status.Active : Status.Inactive);

            var isDeletedConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted);

            builder.ToTable("RoleMenuPrivilege", "AppSecurity");
            builder.HasKey(rm => rm.Id);
            builder.Property(rm => rm.Id).ValueGeneratedOnAdd();

            builder.Property(rm => rm.RoleId)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(rm => rm.MenuId)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(rm => rm.CanView)
                .IsRequired()
                .HasColumnType("bit");

            builder.Property(rm => rm.CanAdd)
                .IsRequired()
                .HasColumnType("bit");

            builder.Property(rm => rm.CanUpdate)
                .IsRequired()
                .HasColumnType("bit");

            builder.Property(rm => rm.CanDelete)
                .IsRequired()
                .HasColumnType("bit");

            builder.Property(rm => rm.CanApprove)
                .IsRequired()
                .HasColumnType("bit");

            builder.Property(rm => rm.CanExport)
                .IsRequired()
                .HasColumnType("bit");

            builder.Property(rm => rm.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(isActiveConverter)
                .IsRequired();

            builder.Property(rm => rm.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeletedConverter)
                .IsRequired();

            builder.Property(rm => rm.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(rm => rm.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("datetime2");

            builder.Property(rm => rm.CreatedByName)
                .HasColumnName("CreatedByName")
                .HasColumnType("varchar(50)");

            builder.Property(rm => rm.CreatedIP)
                .HasColumnName("CreatedIP")
                .HasColumnType("varchar(255)");

            builder.Property(rm => rm.ModifiedBy)
                .HasColumnName("ModifiedBy")
                .HasColumnType("int");

            builder.Property(rm => rm.ModifiedAt)
                .HasColumnName("ModifiedAt")
                .HasColumnType("datetime2");

            builder.Property(rm => rm.ModifiedByName)
                .HasColumnName("ModifiedByName")
                .HasColumnType("varchar(50)");

            builder.Property(rm => rm.ModifiedIP)
                .HasColumnName("ModifiedIP")
                .HasColumnType("varchar(255)");

            builder.HasOne(rm => rm.Menu)
                .WithMany(m => m.RoleMenus)
                .HasForeignKey(rm => rm.MenuId);

            builder.HasOne(rm => rm.UserRole)
                .WithMany(ur => ur.RoleMenuPrivileges)
                .HasForeignKey(rm => rm.RoleId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
