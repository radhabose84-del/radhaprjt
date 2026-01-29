using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings
{
    public class AdminSecuritySettingsDto
    {
        public int Id { get; set; }
        public int PasswordHistoryCount { get; set; }
        public int SessionTimeoutMinutes { get; set; }
        public int MaxFailedLoginAttempts { get; set; }
        public int AccountAutoUnlockMinutes { get; set; }
        public int PasswordExpiryDays { get; set; }
        public int PasswordExpiryAlertDays { get; set; }
        public byte IsTwoFactorAuthenticationEnabled { get; set; }
        public int MaxConcurrentLogins { get; set; }
        public byte IsForcePasswordChangeOnFirstLogin { get; set; }
        public byte PasswordResetCodeExpiryMinutes { get; set; }
        public byte IsCaptchaEnabledOnLogin { get; set; }
        
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }

    }
}