using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
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

            builder.ToTable("Company", "AppData");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(c => c.CompanyName)
                .HasColumnName("CompanyName")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(c => c.LegalName)
                .HasColumnName("LegalName")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(c => c.GstNumber)
                .HasColumnName("GstNumber")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(c => c.TIN)
                .HasColumnName("TIN")
                .HasColumnType("varchar(50)");

            builder.Property(c => c.TAN)
                .HasColumnName("TAN")
                .HasColumnType("varchar(50)");

            builder.Property(c => c.CSTNo)
                .HasColumnName("CSTNo")
                .HasColumnType("varchar(50)");

            builder.Property(c => c.YearOfEstablishment)
                .HasColumnName("YearOfEstablishment")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(c => c.Website)
                .HasColumnName("Website")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(ca => ca.Logo)
                .HasColumnName("Logo")
                .HasColumnType("nvarchar(255)");

            builder.Property(c => c.EntityId)
                .HasColumnName("EntityId")
                .HasColumnType("int")
                .IsRequired();
                
            builder.Property(c => c.PanNumber)
            .HasColumnName("PanNumber")
            .HasColumnType("varchar(10)")
            .IsRequired();

            builder.Property(u => u.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(isActiveConverter)
                .IsRequired();

            builder.Property(u => u.IsDeleted)
       .HasColumnName("IsDeleted")
       .HasColumnType("bit")
       .HasConversion(isDeletedConverter)
       .IsRequired();

            builder.Property(b => b.CreatedByName)
            .IsRequired()
            .HasColumnType("varchar(50)");

            builder.Property(b => b.CreatedIP)
           .IsRequired()
           .HasColumnType("varchar(255)");

            builder.Property(b => b.ModifiedByName)
            .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
            .HasColumnType("varchar(255)");

            builder.HasOne(c => c.CompanyAddress)
                .WithOne(ca => ca.Company)
                .HasForeignKey<CompanyAddress>(ca => ca.CompanyId);

            builder.HasOne(c => c.CompanyContact)
                .WithOne(cc => cc.Company)
                .HasForeignKey<CompanyContact>(cc => cc.CompanyId);
        }
    }
}