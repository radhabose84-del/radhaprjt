using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IAdminSecuritySettings
{
    public interface IAdminSecuritySettingsCommandRepository
    {
        Task<UserManagement.Domain.Entities.AdminSecuritySettings> CreateAsync(UserManagement.Domain.Entities.AdminSecuritySettings adminSecuritySettings);

          Task<int> UpdateAsync(int id, UserManagement.Domain.Entities.AdminSecuritySettings adminSecuritySettings);
        
          
         Task<int> DeleteAsync(int id, UserManagement.Domain.Entities.AdminSecuritySettings adminSecuritySettings); 
    }
}