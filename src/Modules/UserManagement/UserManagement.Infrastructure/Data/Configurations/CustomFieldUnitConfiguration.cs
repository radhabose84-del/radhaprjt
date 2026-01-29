using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class CustomFieldUnitConfiguration : IEntityTypeConfiguration<CustomFieldUnit>
    {
        public void Configure(EntityTypeBuilder<CustomFieldUnit> builder)
        {
            builder.ToTable("CustomFieldUnit", "AppData");
            builder.HasKey(cf => cf.Id);

            builder.Property(cf => cf.CustomFieldId)
                .HasColumnName("CustomFieldId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(cf => cf.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(cf => cf.CustomField)
                .WithMany(ur => ur.CustomFieldUnits)
                .HasForeignKey(cf => cf.CustomFieldId);

            builder.HasOne(cf => cf.Unit)
                .WithMany(ur => ur.CustomFieldUnits)
                .HasForeignKey(cf => cf.UnitId);
        }
    }
}