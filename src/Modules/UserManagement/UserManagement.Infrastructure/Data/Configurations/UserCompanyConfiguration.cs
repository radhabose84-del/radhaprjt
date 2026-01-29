using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class UserCompanyConfiguration : IEntityTypeConfiguration<UserCompany>
    {
        public void Configure(EntityTypeBuilder<UserCompany> builder)
        {
            builder.ToTable("UserCompany", "AppSecurity");
            builder.HasKey(uc => uc.Id);

            builder.Property(uc => uc.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(uc => uc.UserId)
                .HasColumnName("UserId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(uc => uc.CompanyId)
                .HasColumnName("CompanyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(u => u.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1,
                    v => v ? (byte)1 : (byte)0
                )
                .IsRequired();

            builder.HasOne(uc => uc.User)
                .WithMany(u => u.UserCompanies)
                .HasForeignKey(uc => uc.UserId)
                .HasPrincipalKey(u => u.UserId); // ✅ Required to avoid Guid mismatch;

            builder.HasOne(uc => uc.Company)
                .WithMany(c => c.UserCompanies)
                .HasForeignKey(uc => uc.CompanyId);
        }
    }
}