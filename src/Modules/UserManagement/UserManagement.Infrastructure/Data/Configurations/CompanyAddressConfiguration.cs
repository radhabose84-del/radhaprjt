using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class CompanyAddressConfiguration : IEntityTypeConfiguration<CompanyAddress>
    {
        public void Configure(EntityTypeBuilder<CompanyAddress> builder)
        {
            builder.ToTable("CompanyAddress", "AppData");

            builder.HasKey(ca => ca.Id);

            builder.Property(ca => ca.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ca => ca.AddressLine1)
                .HasColumnName("AddressLine1")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(ca => ca.AddressLine2)
                .HasColumnName("AddressLine2")
                .HasColumnType("varchar(100)");

            builder.Property(ca => ca.CityId)
                .HasColumnName("CityId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ca => ca.StateId)
                .HasColumnName("StateId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ca => ca.CountryId)
                .HasColumnName("CountryId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ca => ca.PinCode)
                .HasColumnName("PinCode")
                .HasColumnType("varchar(10)")
                .IsRequired();

                builder.Property(ca => ca.Phone)
                .HasColumnName("Phone")
                .HasColumnType("varchar(20)");

                builder.Property(ca => ca.AlternatePhone)
                .HasColumnName("AlternatePhone")
                .HasColumnType("varchar(20)");

            builder.Property(ca => ca.CompanyId)
                .HasColumnName("CompanyId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(ca => ca.Company)
                .WithOne(c => c.CompanyAddress)
                .HasForeignKey<CompanyAddress>(ca => ca.CompanyId);
        }
    }
}