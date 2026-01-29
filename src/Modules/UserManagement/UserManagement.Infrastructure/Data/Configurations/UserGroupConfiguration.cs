using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static Core.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
    {
        public void Configure(EntityTypeBuilder<UserGroup> builder)
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

              

            builder.ToTable("UserGroup", "AppSecurity");
            builder.HasKey(ug => ug.Id);

            builder.Property(ug => ug.Id)
            .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ug => ug.GroupCode)
            .HasColumnName("GroupCode")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(ug => ug.GroupName)
            .HasColumnName("GroupName")
                .HasColumnType("varchar(100)")
                .IsRequired();

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