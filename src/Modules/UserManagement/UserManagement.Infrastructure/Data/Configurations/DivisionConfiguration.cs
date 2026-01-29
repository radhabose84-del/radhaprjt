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
    public class DivisionConfiguration : IEntityTypeConfiguration<Division>
    {
        public void Configure(EntityTypeBuilder<Division> builder)
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
                
            builder.ToTable("Division", "AppData");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.ShortName)
            .IsRequired()
            .HasColumnType("varchar(50)");

            builder.Property(d => d.Name)
            .IsRequired()
            .HasColumnType("varchar(100)");

            builder.Property(d => d.CompanyId)
            .IsRequired()
            .HasColumnType("int");

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

              builder.HasOne(s => s.Company)
                .WithMany(c => c.Divisions)
                .HasForeignKey(s => s.CompanyId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}