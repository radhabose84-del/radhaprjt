using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;  

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class CompanyContactConfiguration : IEntityTypeConfiguration<CompanyContact>
    {
        public void Configure(EntityTypeBuilder<CompanyContact> builder)
        {
            builder.ToTable("CompanyContact", "AppData");

            builder.HasKey(ca => ca.Id);

            builder.Property(ca => ca.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ca => ca.Name)
                .HasColumnName("Name")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(ca => ca.Phone)
                .HasColumnName("Phone")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(ca => ca.Designation)
                .HasColumnName("Designation")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(ca => ca.Email)
                .HasColumnName("Email")
                .HasColumnType("varchar(20)")
                .IsRequired();

            builder.Property(ca => ca.Remarks)
                .HasColumnName("Remark")
                .HasColumnType("varchar(100)");

            builder.Property(ca => ca.CompanyId)
                .HasColumnName("CompanyId")
                .HasColumnType("int")
                .IsRequired();

            builder.HasOne(c => c.Company)
                .WithOne(cc => cc.CompanyContact)
                .HasForeignKey<CompanyContact>(cc => cc.CompanyId);
        }
    }
}