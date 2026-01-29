using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Entities.Power;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Infrastructure.Data.Configurations.Power
{
    public class FeederGroupConfiguration  : IEntityTypeConfiguration<FeederGroup>
    {

        public void Configure(EntityTypeBuilder<FeederGroup> builder)
        {


            var statusConverter = new ValueConverter<Status, bool>(
                  v => v == Status.Active,
                  v => v ? Status.Active : Status.Inactive
              );
            // ValueConverter for IsDelete (enum to bit)
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted
            );

            builder.ToTable("FeederGroup", "Maintenance");


            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(t => t.FeederGroupCode)
                  .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(t => t.FeederGroupName)
                .HasColumnType("varchar(250)")
                .IsRequired();

                builder.Property(t => t.UnitId)
            .HasColumnName("UnitId")
            .HasColumnType("int")
            .IsRequired();

            builder.Property(t => t.IsActive)
            .HasColumnType("bit")
            .HasConversion(statusConverter)
            .IsRequired();


            builder.Property(t => t.IsDeleted)
            .HasColumnType("bit")
            .IsRequired()
            .HasConversion(isDeleteConverter);
            
             builder.Property(t => t.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(t => t.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(t => t.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(t => t.ModifiedIP)
                .HasColumnType("varchar(50)");                           




        }

        
    }
}