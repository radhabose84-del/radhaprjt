using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class UserUnitConfiguration : IEntityTypeConfiguration<UserUnit>
    {
        public void Configure(EntityTypeBuilder<UserUnit> builder)
        {
            builder.ToTable("UserUnit", "AppSecurity");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(u => u.UnitId)
                .HasColumnName("UnitId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(u => u.UserId)
                .HasColumnName("UserId")
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

                builder.HasOne(uu => uu.Unit)
                .WithMany(u => u.UserUnits)
                .HasForeignKey(uu => uu.UnitId);

            builder.HasOne(uu => uu.User)
                .WithMany(u => u.UserUnits)
                .HasForeignKey(uu => uu.UserId)
                .HasPrincipalKey(u => u.UserId); // ✅ Required to avoid Guid mismatch;


        }
    }
}