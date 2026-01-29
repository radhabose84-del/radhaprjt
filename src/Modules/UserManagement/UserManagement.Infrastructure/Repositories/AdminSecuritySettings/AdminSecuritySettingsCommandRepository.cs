using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IAdminSecuritySettings;
using Core.Application.Common.Interfaces;

namespace UserManagement.Infrastructure.Repositories.AdminSecuritySettings
{
    public class AdminSecuritySettingsCommandRepository  : IAdminSecuritySettingsCommandRepository 
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IIPAddressService _ipAddressService;

         public  AdminSecuritySettingsCommandRepository(ApplicationDbContext applicationDbContext,IIPAddressService iPAddressService)
    {
        _applicationDbContext=applicationDbContext;
        _ipAddressService=iPAddressService;
    } 
     public async Task<Core.Domain.Entities.AdminSecuritySettings> CreateAsync(Core.Domain.Entities.AdminSecuritySettings adminSecuritySettings)
    {       
            var entityId = _ipAddressService.GetEntityId();
            adminSecuritySettings.EntityId = entityId;
            await _applicationDbContext.AdminSecuritySettings.AddAsync(adminSecuritySettings);
            await _applicationDbContext.SaveChangesAsync();
            return adminSecuritySettings;
    }    
     public async Task<int>UpdateAsync(int id, Core.Domain.Entities.AdminSecuritySettings adminSecuritySettings)
    {
            var existingadminsettings = await _applicationDbContext.AdminSecuritySettings.FirstOrDefaultAsync(u => u.Id == id);
                    if (existingadminsettings == null)
                {
                    return -1; //indicate failure
                }
           
                  existingadminsettings.PasswordHistoryCount=adminSecuritySettings.PasswordHistoryCount;
                  existingadminsettings.SessionTimeoutMinutes= adminSecuritySettings.SessionTimeoutMinutes;
                  existingadminsettings.MaxFailedLoginAttempts=adminSecuritySettings.MaxFailedLoginAttempts;
                  existingadminsettings.AccountAutoUnlockMinutes=adminSecuritySettings.AccountAutoUnlockMinutes;
                  existingadminsettings.PasswordExpiryDays=adminSecuritySettings.PasswordExpiryDays;
                  existingadminsettings.PasswordExpiryAlertDays=adminSecuritySettings.PasswordExpiryAlertDays;
                  existingadminsettings.IsTwoFactorAuthenticationEnabled=adminSecuritySettings.IsTwoFactorAuthenticationEnabled;
                  existingadminsettings.MaxConcurrentLogins=adminSecuritySettings.MaxConcurrentLogins;
                  existingadminsettings.IsForcePasswordChangeOnFirstLogin=adminSecuritySettings.IsForcePasswordChangeOnFirstLogin;
                  existingadminsettings.PasswordResetCodeExpiryMinutes=adminSecuritySettings.PasswordResetCodeExpiryMinutes;
                  existingadminsettings.PasswordResetCodeExpiryMinutes=adminSecuritySettings.PasswordResetCodeExpiryMinutes;
                  existingadminsettings.IsCaptchaEnabledOnLogin=adminSecuritySettings.IsCaptchaEnabledOnLogin;
                  existingadminsettings.IsActive=adminSecuritySettings.IsActive;
                


                _applicationDbContext.AdminSecuritySettings.Update(existingadminsettings);
                 await _applicationDbContext.SaveChangesAsync();
            
            return 1; // No user found
    }

        public async Task<int> DeleteAsync(int id ,Core.Domain.Entities.AdminSecuritySettings adminSecuritySettings )
    {
        
            var adminsettingsToDelete = await _applicationDbContext.AdminSecuritySettings.FirstOrDefaultAsync(u => u.Id == id);
             if (adminsettingsToDelete == null)
                {
                    return -1; //indicate failure
                }            
                adminsettingsToDelete.IsDeleted = adminSecuritySettings.IsDeleted;
                _applicationDbContext.AdminSecuritySettings.Update(adminsettingsToDelete);
                 await _applicationDbContext.SaveChangesAsync();
                 return 1;
           
         
    }

   




    

        
    }
}