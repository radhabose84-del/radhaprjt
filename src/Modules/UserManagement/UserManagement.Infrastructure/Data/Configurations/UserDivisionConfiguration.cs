using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class UserDivisionConfiguration : IEntityTypeConfiguration<UserDivision>
    {
        public void Configure(EntityTypeBuilder<UserDivision> builder)
        {
            builder.ToTable("UserDivision", "AppSecurity");

            builder.HasKey(ura => ura.Id);

            builder.Property(ura => ura.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(ura => ura.DivisionId)
                .HasColumnName("DivisionId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(ura => ura.UserId)
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

            builder.HasOne(ura => ura.User)
                .WithMany(ur => ur.UserDivisions)
                .HasForeignKey(ura => ura.UserId)
                .HasPrincipalKey(ur => ur.UserId); // ✅ Added to avoid Guid mismatch

            builder.HasOne(ura => ura.Division)
                .WithMany(u => u.UserDivisions)
                .HasForeignKey(ura => ura.DivisionId);
        }
    }
}