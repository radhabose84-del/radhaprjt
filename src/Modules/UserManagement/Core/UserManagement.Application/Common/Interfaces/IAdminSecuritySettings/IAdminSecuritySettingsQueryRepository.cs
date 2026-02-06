using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IAdminSecuritySettings
{
    public interface IAdminSecuritySettingsQueryRepository 
    {
        // Task<List<UserManagement.Domain.Entities.AdminSecuritySettings>> GetAllAdminSecuritySettingsAsync();

        Task<(List<UserManagement.Domain.Entities.AdminSecuritySettings>,int)> GetAllAdminSecuritySettingsAsync(int PageNumber, int PageSize, string? SearchTerm);
         Task<UserManagement.Domain.Entities.AdminSecuritySettings> GetAdminSecuritySettingsByIdAsync(int id);
         
         
    }
}