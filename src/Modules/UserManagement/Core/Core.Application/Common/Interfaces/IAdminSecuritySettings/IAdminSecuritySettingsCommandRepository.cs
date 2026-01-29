using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.IAdminSecuritySettings
{
    public interface IAdminSecuritySettingsCommandRepository
    {
        Task<Core.Domain.Entities.AdminSecuritySettings> CreateAsync(Core.Domain.Entities.AdminSecuritySettings adminSecuritySettings);

          Task<int> UpdateAsync(int id, Core.Domain.Entities.AdminSecuritySettings adminSecuritySettings);
        
          
         Task<int> DeleteAsync(int id, Core.Domain.Entities.AdminSecuritySettings adminSecuritySettings); 
    }
}