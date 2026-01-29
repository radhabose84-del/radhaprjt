using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class CustomFieldOptionalValueConfiguration : IEntityTypeConfiguration<CustomFieldOptionalValue>
    {
        public void Configure(EntityTypeBuilder<CustomFieldOptionalValue> builder)
        {
            builder.ToTable("CustomFieldOptionalValue", "AppData");
            builder.HasKey(cf => cf.Id);

            builder.Property(cf => cf.CustomFieldId)
                .HasColumnName("CustomFieldId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(cf => cf.OptionFieldValue)
                .HasColumnName("OptionFieldValue")
                .HasColumnType("varchar(200)")
                .IsRequired();

            builder.HasOne(cf => cf.CustomField)
                .WithMany(ur => ur.CustomFieldOptionalValues)
                .HasForeignKey(cf => cf.CustomFieldId);
        }
    }
}