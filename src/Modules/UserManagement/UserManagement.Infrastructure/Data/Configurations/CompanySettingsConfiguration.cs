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
    public class CompanySettingsConfiguration : IEntityTypeConfiguration<CompanySettings>
    {
        public void Configure(EntityTypeBuilder<CompanySettings> builder)
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
                
            builder.ToTable("CompanySetting", "AppData");
            builder.HasKey(m => m.Id);
            builder.HasIndex(c => new { c.CompanyId, c.CurrencyId,c.FinancialYearId,c.LanguageId }).IsUnique(false);

            builder.Property(c => c.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(c => c.CompanyId)
                .HasColumnName("CompanyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(c => c.PasswordHistoryCount)
                .HasColumnName("PasswordHistoryCount")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(c => c.SessionTimeout)
                .HasColumnName("SessionTimeout")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(c => c.FailedLoginAttempts)
                .HasColumnName("FailedLoginAttempts")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(c => c.AutoReleaseTime)
                .HasColumnName("AutoReleaseTime")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(c => c.PasswordExpiryDays)
                .HasColumnName("PasswordExpiryDays")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(c => c.PasswordExpiryAlert)
                .HasColumnName("PasswordExpiryAlert")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(c => c.TwoFactorAuth)
                .HasColumnName("TwoFactorAuth")
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired();

            builder.Property(c => c.MaxConcurrentLogins)
                .HasColumnName("MaxConcurrentLogins")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(c => c.ForgotPasswordCodeExpiry)
                .HasColumnName("ForgotPasswordCodeExpiry")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(c => c.CaptchaOnLogin)
                .HasColumnName("CaptchaOnLogin")
                .HasColumnType("bit")
                .HasConversion(
                    v => v == 1, 
                    v => v ? (byte)1 : (byte)0 
                )
                .IsRequired();

            builder.Property(c => c.CurrencyId)
                .HasColumnName("CurrencyId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(c => c.LanguageId)
                .HasColumnName("LanguageId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(c => c.TimeZone)
                .HasColumnName("TimeZone")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(c => c.FinancialYearId)
                .HasColumnName("FinancialYearId")
                .HasColumnType("int")
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

             builder.HasOne(ca => ca.Currency)
                .WithOne(ca => ca.CompanySettings)
                .HasForeignKey<CompanySettings>(ca => ca.CurrencyId);

            builder.HasOne(ca => ca.Language)
                .WithOne(ca => ca.CompanySettings)
                .HasForeignKey<CompanySettings>(ca => ca.LanguageId);

            builder.HasOne(ca => ca.FinancialYear)
                .WithOne(ca => ca.CompanySettings)
                .HasForeignKey<CompanySettings>(ca => ca.FinancialYearId);

                 builder.HasOne(ca => ca.company)
                .WithOne(ca => ca.CompanySettings)
                .HasForeignKey<CompanySettings>(ca => ca.CompanyId);
        }
    }
}