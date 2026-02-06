using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.Infrastructure.Data.Configurations
{
    public class MiscTypeMasterConfiguration : IEntityTypeConfiguration<MiscTypeMaster>
    {
        public void Configure(EntityTypeBuilder<MiscTypeMaster> builder)
        {

            var statusConverter = new ValueConverter<Status, bool>(
                 v => v == Status.Active,                    // Convert to DB (1 for Active)
                 v => v ? Status.Active : Status.Inactive    // Convert to Entity
             );

            // ValueConverter for IsDelete (enum to bit)
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 // Convert to DB (1 for Deleted)
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted // Convert to Entity
            );   
            
             builder.ToTable("MiscTypeMaster", "Party");

                // Primary Key
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            // Code column
             builder.Property(m => m.MiscTypeCode)
                .HasColumnName("MiscTypeCode")
                .HasColumnType("nvarchar(50)")
                .IsRequired();

            // Description column
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