using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class RoleChildConfiguration : IEntityTypeConfiguration<RoleChild>
    {
        public void Configure(EntityTypeBuilder<RoleChild> builder)
        {
            builder.ToTable("RoleChild", "AppSecurity");
            builder.HasKey(rc => rc.Id);
            builder.Property(rc => rc.Id)
                .ValueGeneratedOnAdd();

            builder.Property(rc => rc.RoleId)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(rc => rc.MenuId)
                .IsRequired()
                .HasColumnType("int");

            builder.HasOne(rc => rc.Role)
                .WithMany(ur => ur.RoleChildren)
                .HasForeignKey(rc => rc.RoleId);

            builder.HasOne(rc => rc.Menu)
                .WithMany(ur => ur.RoleChildren)
                .HasForeignKey(rc => rc.MenuId);
        }
    }
}