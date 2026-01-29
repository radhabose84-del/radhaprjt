using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.API.Validation.Common;
using Core.Application.AdminSecuritySettings.Commands.UpdateAdminSecuritySettings;
using FluentValidation;

namespace UserManagement.API.Validation.AdminSecuritySettings
{
    public class UpdateAdminSecuritySettingsCommandValidator : AbstractValidator<UpdateAdminSecuritySettingsCommand>
    {
        
           public UpdateAdminSecuritySettingsCommandValidator()
           {   
            
            RuleFor(x => x.PasswordHistoryCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Password history count must be zero or greater.");

            RuleFor(x => x.SessionTimeoutMinutes)
                .GreaterThan(0)
                .WithMessage("Session timeout must be greater than zero.");

            RuleFor(x => x.MaxFailedLoginAttempts)
                .InclusiveBetween(1, 10)
                .WithMessage("Max failed login attempts must be between 1 and 10.");

            RuleFor(x => x.AccountAutoUnlockMinutes)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Account auto-unlock minutes must be zero or greater.");

            RuleFor(x => x.PasswordExpiryDays)
                .GreaterThan(0)
                .WithMessage("Password expiry days must be greater than zero.");

            RuleFor(x => x.PasswordExpiryAlertDays)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Password expiry alert days must be zero or greater.")
                .LessThan(x => x.PasswordExpiryDays)
                .WithMessage("Password expiry alert days must be less than password expiry days.");

            RuleFor(x => x.IsTwoFactorAuthenticationEnabled)
                .InclusiveBetween((byte)0, (byte)1)
                .WithMessage("Two-factor authentication must be either 0 (disabled) or 1 (enabled).");

            RuleFor(x => x.MaxConcurrentLogins)
                .GreaterThan(0)
                .WithMessage("Max concurrent logins must be greater than zero.");

            RuleFor(x => x.IsForcePasswordChangeOnFirstLogin)
                .InclusiveBetween((byte)0, (byte)1)
                .WithMessage("Force password change on first login must be either 0 (disabled) or 1 (enabled).");

            RuleFor(x => x.PasswordResetCodeExpiryMinutes)
                 .GreaterThan(0)
                .WithMessage("Password reset code expiry minutes must be greater than zero.");

            RuleFor(x => x.IsCaptchaEnabledOnLogin)
                .InclusiveBetween((byte)0, (byte)1)
                .WithMessage("Captcha enabled on login must be either 0 (disabled) or 1 (enabled).");

           }

    }
}