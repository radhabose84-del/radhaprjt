using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class RoleEntitlementConfigurations : IEntityTypeConfiguration<RoleEntitlement>
    {
    public void Configure(EntityTypeBuilder<RoleEntitlement> builder)
    {
           // Table Name and Schema
            builder.ToTable("RoleEntitlements", "AppSecurity");

            // Primary Key
            builder.HasKey(re => re.Id);
            builder.Property(re => re.Id).ValueGeneratedOnAdd();


            // Relationships

            // Role Relationship
            // builder.HasOne(re => re.UserRole)
            //     .WithMany(r => r.RoleEntitlements)
            //     .HasForeignKey(re => re.UserRoleId)
            //     .OnDelete(DeleteBehavior.Cascade); // Deletes role entitlements when a role is deleted

            // Module Relationship
            // builder.HasOne(re => re.Module)
            //     .WithMany(m => m.RoleEntitlements)
            //     .HasForeignKey(re => re.ModuleId)
            //     .OnDelete(DeleteBehavior.Restrict); // Restricts deletion of a module if entitlements exist

            // Menu Relationship
            // builder.HasOne(re => re.Menu)
            //     .WithMany(me => me.RoleEntitlements)
            //     .HasForeignKey(re => re.MenuId)
            //     .OnDelete(DeleteBehavior.Restrict); // Restricts deletion of a menu if entitlements exist

            // Default Values for Permissions
            builder.Property(re => re.CanView).HasDefaultValue(false);
            builder.Property(re => re.CanAdd).HasDefaultValue(false);
            builder.Property(re => re.CanUpdate).HasDefaultValue(false);
            builder.Property(re => re.CanDelete).HasDefaultValue(false);
            builder.Property(re => re.CanExport).HasDefaultValue(false);
            builder.Property(re => re.CanApprove).HasDefaultValue(false);


    }
    }
}