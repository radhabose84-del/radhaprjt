using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Domain.Entities;

namespace UserManagement.Infrastructure.Data.Configurations
{
    public class AdminSecuritySettingsConfiguration :IEntityTypeConfiguration<AdminSecuritySettings>
     {
         public void Configure(EntityTypeBuilder<AdminSecuritySettings> builder)
    {
        builder.ToTable("AdminSecuritySettings", "AppSecurity");        
        
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("Id")
            .HasColumnType("int")
            .IsRequired(true);

        builder.Property(p => p.PasswordHistoryCount)
            .HasColumnName("PasswordHistoryCount")
            .HasColumnType("int")
            .IsRequired(true);

        builder.Property(p => p.SessionTimeoutMinutes)
            .HasColumnName("SessionTimeoutMinutes")
            .HasColumnType("int")
            .IsRequired(true);

        builder.Property(p => p.MaxFailedLoginAttempts)
            .HasColumnName("MaxFailedLoginAttempts")
            .HasColumnType("int")
            .IsRequired(true);

        builder.Property(p => p.AccountAutoUnlockMinutes)
            .HasColumnName("AccountAutoUnlockMinutes")
            .HasColumnType("int")
            .IsRequired(true);

        builder.Property(p => p.PasswordExpiryDays)
            .HasColumnName("PasswordExpiryDays")
            .HasColumnType("int")
            .IsRequired(true);

        builder.Property(p => p.PasswordExpiryAlertDays)
            .HasColumnName("PasswordExpiryAlertDays")
            .HasColumnType("int")
            .IsRequired(true);        
            
            builder.Property(p => p.IsTwoFactorAuthenticationEnabled)
            .HasColumnName("IsTwoFactorAuthenticationEnabled")
            .HasColumnType("bit")
            .HasConversion(
                v => v == 1, // convert byte to bool
                v => v ? (byte)1 : (byte)0 // convert bool to byte
            )
            .IsRequired();    

        builder.Property(p => p.MaxConcurrentLogins)
            .HasColumnName("MaxConcurrentLogins")
            .HasColumnType("int")
            .IsRequired(true);
        
        

            builder.Property(p => p.IsForcePasswordChangeOnFirstLogin)
            .HasColumnName("IsForcePasswordChangeOnFirstLogin")
            .HasColumnType("bit")
            .HasConversion(
                v => v == 1, // convert byte to bool
                v => v ? (byte)1 : (byte)0 // convert bool to byte
            )
            .IsRequired();

       

        builder.Property(p => p.PasswordResetCodeExpiryMinutes)
            .HasColumnName("PasswordResetCodeExpiryMinutes")
            .HasColumnType("int")
            .IsRequired(true);

             builder.Property(p => p.IsCaptchaEnabledOnLogin)
            .HasColumnName("IsCaptchaEnabledOnLogin")
            .HasColumnType("bit")
            .HasConversion(
                v => v == 1, // convert byte to bool
                v => v ? (byte)1 : (byte)0 // convert bool to byte
            )
            .IsRequired();

             builder.Property(p => p.EntityId)
            .HasColumnName("EntityId")
            .HasColumnType("int")
            .IsRequired(false);

            builder.HasOne(p => p.entity)
                .WithOne(e => e.AdminSecuritySettings)
                .HasForeignKey<AdminSecuritySettings>(p => p.EntityId)
                .OnDelete(DeleteBehavior.SetNull);



    }
    }
}