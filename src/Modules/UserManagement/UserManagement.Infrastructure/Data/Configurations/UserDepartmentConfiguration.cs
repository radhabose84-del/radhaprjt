using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class UserDepartmentConfiguration : IEntityTypeConfiguration<UserDepartment>
    {
        public void Configure(EntityTypeBuilder<UserDepartment> builder)
        {
            builder.ToTable("UserDepartment", "AppSecurity");
            builder.HasKey(ud => ud.Id);
            
            builder.Property(ud => ud.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ud => ud.UserId)
                .HasColumnName("UserId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ud => ud.DepartmentId)
                .HasColumnName("DepartmentId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(u => u.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0
                );

            builder.HasOne(ud => ud.User)
            .WithMany(u => u.UserDepartments)
            .HasForeignKey(ud => ud.UserId)
            .HasPrincipalKey(u => u.UserId); // ✅ Required to avoid Guid mismatch;

            builder.HasOne(ud => ud.Department)
            .WithMany(d => d.UserDepartments)
            .HasForeignKey(ud => ud.DepartmentId);
        }
    }
}