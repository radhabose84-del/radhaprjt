using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;


namespace Core.Domain.Entities
{
    public class AdminSecuritySettings : BaseEntity
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
    public int PasswordResetCodeExpiryMinutes { get; set; }
    public byte IsCaptchaEnabledOnLogin { get; set; }
    public int? EntityId { get; set; }
    public Entity? entity { get; set; }
      
    }
}        