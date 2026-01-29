using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class RoleParentConfiguration : IEntityTypeConfiguration<RoleParent>
    {
        public void Configure(EntityTypeBuilder<RoleParent> builder)
        {
            builder.ToTable("RoleParent", "AppSecurity");
            builder.HasKey(rp => rp.Id);
            builder.Property(rp => rp.Id)
                .ValueGeneratedOnAdd();

            builder.Property(rp => rp.RoleId)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(rp => rp.MenuId)
                .IsRequired()
                .HasColumnType("int");

            builder.HasOne(rp => rp.Role)
                .WithMany(ur => ur.RoleParents)
                .HasForeignKey(rp => rp.RoleId);

            builder.HasOne(rp => rp.Menu)
                .WithMany(ur => ur.RoleParents)
                .HasForeignKey(rp => rp.MenuId);
        }
    }
}