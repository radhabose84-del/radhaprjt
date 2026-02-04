using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterCommandRepository
    {
        
        Task<UserManagement.Domain.Entities.MiscMaster> CreateAsync(UserManagement.Domain.Entities.MiscMaster miscMaster);  

        Task<int> GetMaxSortOrderAsync();

        Task<bool> UpdateAsync(int id, UserManagement.Domain.Entities.MiscMaster miscMaster);

        Task<bool> DeleteAsync(int id,UserManagement.Domain.Entities.MiscMaster miscMaster); 
    }
}