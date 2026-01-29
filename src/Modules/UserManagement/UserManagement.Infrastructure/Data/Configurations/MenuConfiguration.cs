using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static Core.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class MenuConfiguration : IEntityTypeConfiguration<Menu>
    {
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
         var isActiveConverter = new ValueConverter<Status, bool>
               (
                    v => v == Status.Active,  
                    v => v ? Status.Active : Status.Inactive 
                );

                var isDeletedConverter = new ValueConverter<IsDelete, bool>
                (
                 v => v == IsDelete.Deleted,  
                 v => v ? IsDelete.Deleted : IsDelete.NotDeleted 
                );

        builder.ToTable("Menus", "AppData");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.MenuName)
        .IsRequired()
        .HasColumnType("varchar(100)");

        builder.Property(d => d.ModuleId)
            .IsRequired()
            .HasColumnType("int");

            builder.Property(d => d.ParentId)
            .HasColumnType("int");

             builder.Property(m => m.MenuUrl)
            .IsRequired()
            .HasColumnType("varchar(255)");

            builder.Property(m => m.MenuIcon)
            .IsRequired(false)
            .HasColumnType("varchar(255)");

            builder.Property(d => d.SortOrder)
            .HasColumnType("int");

            builder.Property(d => d.Type)
            .IsRequired(false)
            .HasColumnType("varchar(255)");

        builder.HasOne(m => m.Module)
               .WithMany(module => module.Menus)
               .HasForeignKey(m => m.ModuleId);

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

             builder.HasOne(rm => rm.Parent)
                .WithMany(m => m.ChildMenus)
                .HasForeignKey(rm => rm.Id);
    }
    }
}