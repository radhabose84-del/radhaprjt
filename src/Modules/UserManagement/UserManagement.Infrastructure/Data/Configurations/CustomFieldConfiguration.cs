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
    public class CustomFieldConfiguration : IEntityTypeConfiguration<CustomField>
    {
        public void Configure(EntityTypeBuilder<CustomField> builder)
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
                
            builder.ToTable("CustomField", "AppData");
            builder.HasKey(cf => cf.Id);
            
            builder.Property(cf => cf.LabelName)
                .HasColumnName("LabelName")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(cf => cf.DataTypeId)
                .HasColumnName("DataTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(cf => cf.Length)
                .HasColumnName("Length")
                .HasColumnType("int");

            builder.Property(cf => cf.LabelTypeId)
                .HasColumnName("LabelTypeId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(cf => cf.IsRequired)
                .HasColumnName("IsRequired")
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired();

            builder.Property(cf => cf.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(isActiveConverter)
                .IsRequired();

            builder.Property(cf => cf.IsDeleted)
                 .HasColumnName("IsDeleted")
                 .HasColumnType("bit")
                 .HasConversion(isDeletedConverter)
                 .IsRequired();

            builder.Property(cf => cf.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");

            builder.Property(cf => cf.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(255)");

            builder.Property(cf => cf.ModifiedByName)
                 .HasColumnType("varchar(50)");

            builder.Property(cf => cf.ModifiedIP)
                .HasColumnType("varchar(255)");

            builder.HasOne(cf => cf.DataType)
                .WithMany(cf => cf.CustomFieldDataTypes)
                .HasForeignKey(cf => cf.DataTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(cf => cf.LabelType)
                .WithMany(cf => cf.CustomFieldLabelTypes)
                .HasForeignKey(cf => cf.LabelTypeId)
                .OnDelete(DeleteBehavior.Restrict);  

        }
    }
}