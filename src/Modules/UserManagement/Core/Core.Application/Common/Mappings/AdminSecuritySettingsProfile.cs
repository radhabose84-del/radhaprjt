using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Domain.Entities;
using Core.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using Core.Application.AdminSecuritySettings.Commands.CreateAdminSecuritySettings;
using Core.Application.AdminSecuritySettings.Commands.DeleteAdminSecuritySettings;
using Core.Application.AdminSecuritySettings.Commands.UpdateAdminSecuritySettings;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
{
    public class AdminSecuritySettingsProfile :Profile
    {
            public AdminSecuritySettingsProfile()
        {
                CreateMap <Core.Domain.Entities.AdminSecuritySettings, GetAdminSecuritySettingsDto>();
                 

                CreateMap<CreateAdminSecuritySettingsCommand, Core.Domain.Entities.AdminSecuritySettings>()
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

                  CreateMap<Core.Domain.Entities.AdminSecuritySettings, AdminSecuritySettingsDto>()             
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
                
                CreateMap<DeleteAdminSecuritySettingsCommand,  Core.Domain.Entities.AdminSecuritySettings>()  
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));   

                CreateMap<UpdateAdminSecuritySettingsCommand, Core.Domain.Entities.AdminSecuritySettings>()                  
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

                CreateMap<AdminSecuritySettingsStatusDto, Core.Domain.Entities.AdminSecuritySettings>();
                
        }
        
    }
}