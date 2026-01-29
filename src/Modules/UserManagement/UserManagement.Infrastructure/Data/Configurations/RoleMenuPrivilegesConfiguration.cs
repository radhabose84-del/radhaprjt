using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class RoleMenuPrivilegesConfiguration : IEntityTypeConfiguration<RoleMenuPrivileges>
    {
        public void Configure(EntityTypeBuilder<RoleMenuPrivileges> builder)
        {
            builder.ToTable("RoleMenuPrivilege", "AppSecurity");
            builder.HasKey(rm => rm.Id);
            builder.Property(rm => rm.Id)
            .ValueGeneratedOnAdd();

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