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
    public class LanguageConfiguration : IEntityTypeConfiguration<Language>
    {
        public void Configure(EntityTypeBuilder<Language> builder)
        {
            builder.ToTable("Language","AppData");

            builder.HasKey(l => l.Id);

              builder.Property(c => c.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(d => d.Code)
            .IsRequired()
            .HasColumnType("varchar(10)");

             builder.Property(d => d.Name)
            .IsRequired()
            .HasColumnType("varchar(50)");

             var isActiveConverter = new ValueConverter<Status, bool>(
        v => v == Status.Active,  
        v => v ? Status.Active : Status.Inactive 
    );

            builder.Property(u => u.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("bit")
            .HasConversion(isActiveConverter)
            .IsRequired();

             var isDeletedConverter = new ValueConverter<IsDelete, bool>(
        v => v == IsDelete.Deleted,  
        v => v ? IsDelete.Deleted : IsDelete.NotDeleted 
    );

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