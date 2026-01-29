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
    public class MiscTypeMasterConfiguration : IEntityTypeConfiguration<MiscTypeMaster>
    {
          public void Configure(EntityTypeBuilder<MiscTypeMaster> builder)
        {
     
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,                    
                v => v ? Status.Active : Status.Inactive    
            );

            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted 
            );

             
              builder.ToTable("MiscTypeMaster", "AppData");

            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

             builder.Property(m => m.MiscTypeCode)
                .HasColumnName("MiscTypeCode")
                .HasColumnType("nvarchar(50)")
                .IsRequired();

            builder.Property(m => m.Description)
                .HasColumnName("Description")
                .HasColumnType("varchar(250)")
                .IsRequired();


                 builder.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

                 builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
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