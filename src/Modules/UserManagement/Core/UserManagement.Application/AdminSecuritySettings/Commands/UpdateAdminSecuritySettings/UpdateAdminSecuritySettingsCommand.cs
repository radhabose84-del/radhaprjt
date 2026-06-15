using MediatR;
using Contracts.Common;

namespace UserManagement.Application.AdminSecuritySettings.Commands.UpdateAdminSecuritySettings
{
    public class UpdateAdminSecuritySettingsCommand :  IRequest<int>, IRequirePermission
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
    public byte IsActive {get; set;}
         public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
