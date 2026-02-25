using AutoMapper;
using UserManagement.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using UserManagement.Application.AdminSecuritySettings.Commands.CreateAdminSecuritySettings;
using UserManagement.Application.AdminSecuritySettings.Commands.DeleteAdminSecuritySettings;
using UserManagement.Application.AdminSecuritySettings.Commands.UpdateAdminSecuritySettings;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class AdminSecuritySettingsProfile :Profile
    {
            public AdminSecuritySettingsProfile()
        {
                CreateMap <UserManagement.Domain.Entities.AdminSecuritySettings, GetAdminSecuritySettingsDto>();
                 

                CreateMap<CreateAdminSecuritySettingsCommand, UserManagement.Domain.Entities.AdminSecuritySettings>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHistoryCount, opt => opt.MapFrom(src => src.PasswordHistoryCount))
                .ForMember(dest => dest.SessionTimeoutMinutes, opt => opt.MapFrom(src => src.SessionTimeoutMinutes))
                .ForMember(dest => dest.MaxFailedLoginAttempts, opt => opt.MapFrom(src => src.MaxFailedLoginAttempts))
                .ForMember(dest => dest.AccountAutoUnlockMinutes, opt => opt.MapFrom(src => src.AccountAutoUnlockMinutes))
                .ForMember(dest => dest.PasswordExpiryDays, opt => opt.MapFrom(src => src.PasswordExpiryDays))
                .ForMember(dest => dest.PasswordExpiryAlertDays, opt => opt.MapFrom(src => src.PasswordExpiryAlertDays))
                .ForMember(dest => dest.IsTwoFactorAuthenticationEnabled, opt => opt.MapFrom(src => src.IsTwoFactorAuthenticationEnabled))
                .ForMember(dest => dest.MaxConcurrentLogins, opt => opt.MapFrom(src => src.MaxConcurrentLogins))
                .ForMember(dest => dest.IsForcePasswordChangeOnFirstLogin, opt => opt.MapFrom(src => src.IsForcePasswordChangeOnFirstLogin))
                .ForMember(dest => dest.PasswordResetCodeExpiryMinutes, opt => opt.MapFrom(src => src.PasswordResetCodeExpiryMinutes))
                .ForMember(dest => dest.IsCaptchaEnabledOnLogin, opt => opt.MapFrom(src => src.IsCaptchaEnabledOnLogin))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

                  CreateMap<UserManagement.Domain.Entities.AdminSecuritySettings, AdminSecuritySettingsDto>()             
              .ForMember(dest => dest.PasswordHistoryCount, opt => opt.MapFrom(src => src.PasswordHistoryCount))
                .ForMember(dest => dest.SessionTimeoutMinutes, opt => opt.MapFrom(src => src.SessionTimeoutMinutes))
                .ForMember(dest => dest.MaxFailedLoginAttempts, opt => opt.MapFrom(src => src.MaxFailedLoginAttempts))
                .ForMember(dest => dest.AccountAutoUnlockMinutes, opt => opt.MapFrom(src => src.AccountAutoUnlockMinutes))
                .ForMember(dest => dest.PasswordExpiryDays, opt => opt.MapFrom(src => src.PasswordExpiryDays))
                .ForMember(dest => dest.PasswordExpiryAlertDays, opt => opt.MapFrom(src => src.PasswordExpiryAlertDays))
                .ForMember(dest => dest.IsTwoFactorAuthenticationEnabled, opt => opt.MapFrom(src => src.IsTwoFactorAuthenticationEnabled))
                .ForMember(dest => dest.MaxConcurrentLogins, opt => opt.MapFrom(src => src.MaxConcurrentLogins))
                .ForMember(dest => dest.IsForcePasswordChangeOnFirstLogin, opt => opt.MapFrom(src => src.IsForcePasswordChangeOnFirstLogin))
                .ForMember(dest => dest.PasswordResetCodeExpiryMinutes, opt => opt.MapFrom(src => src.PasswordResetCodeExpiryMinutes))
                .ForMember(dest => dest.IsCaptchaEnabledOnLogin, opt => opt.MapFrom(src => src.IsCaptchaEnabledOnLogin));
                
                CreateMap<DeleteAdminSecuritySettingsCommand,  UserManagement.Domain.Entities.AdminSecuritySettings>()  
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));   

                CreateMap<UpdateAdminSecuritySettingsCommand, UserManagement.Domain.Entities.AdminSecuritySettings>()                  
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

                CreateMap<AdminSecuritySettingsStatusDto, UserManagement.Domain.Entities.AdminSecuritySettings>();
                
        }
        
    }
}