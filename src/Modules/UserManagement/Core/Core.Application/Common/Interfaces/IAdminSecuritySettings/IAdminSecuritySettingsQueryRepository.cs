using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.IAdminSecuritySettings
{
    public interface IAdminSecuritySettingsQueryRepository 
    {
        // Task<List<Core.Domain.Entities.AdminSecuritySettings>> GetAllAdminSecuritySettingsAsync();

        Task<(List<Core.Domain.Entities.AdminSecuritySettings>,int)> GetAllAdminSecuritySettingsAsync(int PageNumber, int PageSize, string? SearchTerm);
         Task<Core.Domain.Entities.AdminSecuritySettings> GetAdminSecuritySettingsByIdAsync(int id);
         
         
    }
}