using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class CustomFieldMenuConfiguration : IEntityTypeConfiguration<CustomFieldMenu>
    {
        public void Configure(EntityTypeBuilder<CustomFieldMenu> builder)
        {
            builder.ToTable("CustomFieldMenu", "AppData");
            builder.HasKey(cf => cf.Id);

            builder.Property(cf => cf.CustomFieldId)
                .HasColumnName("CustomFieldId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(cf => cf.MenuId)
                .HasColumnName("MenuId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(cf => cf.CustomField)
                .WithMany(ur => ur.CustomFieldMenu)
                .HasForeignKey(cf => cf.CustomFieldId);

            builder.HasOne(cf => cf.Menu)
                .WithMany(ur => ur.CustomFieldMenus)
                .HasForeignKey(cf => cf.MenuId);

        }
    }
}