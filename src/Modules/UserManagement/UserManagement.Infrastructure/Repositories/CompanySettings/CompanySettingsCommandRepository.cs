using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Infrastructure.Data;
using Core.Application.Common.Interfaces.ICompanySettings;

namespace UserManagement.Infrastructure.Repositories.CompanySettings
{
    public class CompanySettingsCommandRepository : ICompanyCommandSettings
    {
        private readonly ApplicationDbContext _context;

        public CompanySettingsCommandRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<int> CreateAsync(Core.Domain.Entities.CompanySettings companySettings)
        {
             await _context.CompanySettings.AddAsync(companySettings);
             await _context.SaveChangesAsync();
            return companySettings.Id;
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateAsync(int id, Core.Domain.Entities.CompanySettings companySettings)
        {
            var companySettingsToUpdate = _context.CompanySettings.FirstOrDefault(p => p.Id == id);
            if (companySettingsToUpdate != null)
            {
                companySettingsToUpdate.CompanyId = companySettings.CompanyId;
                companySettingsToUpdate.PasswordHistoryCount = companySettings.PasswordHistoryCount;
                companySettingsToUpdate.SessionTimeout = companySettings.SessionTimeout;
                companySettingsToUpdate.FailedLoginAttempts = companySettings.FailedLoginAttempts;
                companySettingsToUpdate.AutoReleaseTime = companySettings.AutoReleaseTime;
                companySettingsToUpdate.PasswordExpiryDays = companySettings.PasswordExpiryDays;
                companySettingsToUpdate.PasswordExpiryAlert = companySettings.PasswordExpiryAlert;
                companySettingsToUpdate.TwoFactorAuth = companySettings.TwoFactorAuth;
                companySettingsToUpdate.MaxConcurrentLogins = companySettings.MaxConcurrentLogins;
                companySettingsToUpdate.ForgotPasswordCodeExpiry = companySettings.ForgotPasswordCodeExpiry;
                companySettingsToUpdate.CaptchaOnLogin = companySettings.CaptchaOnLogin;
                companySettingsToUpdate.CurrencyId = companySettings.CurrencyId;
                companySettingsToUpdate.LanguageId = companySettings.LanguageId;
                companySettingsToUpdate.TimeZone = companySettings.TimeZone;
                companySettingsToUpdate.FinancialYearId = companySettings.FinancialYearId;
                companySettingsToUpdate.IsActive = companySettings.IsActive;
                _context.CompanySettings.Update(companySettingsToUpdate);
              return await  _context.SaveChangesAsync() > 0;
                
            }
            return false;
        }
    }
}