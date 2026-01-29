using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class RoleModuleConfiguration : IEntityTypeConfiguration<RoleModule>
    {
        public void Configure(EntityTypeBuilder<RoleModule> builder)
        {
            builder.ToTable("RoleModule", "AppSecurity");
            builder.HasKey(rm => rm.Id);
            builder.Property(rm => rm.Id)
                .ValueGeneratedOnAdd();

            builder.Property(rm => rm.RoleId)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(rm => rm.ModuleId)
                .IsRequired()
                .HasColumnType("int");

                builder.HasOne(rm => rm.Role)
                .WithMany(m => m.RoleModules)
                .HasForeignKey(rm => rm.RoleId);

                builder.HasOne(rm => rm.Module)
                .WithMany(ur => ur.RoleModules)
                .HasForeignKey(rm => rm.ModuleId);

              
        }
    }
}