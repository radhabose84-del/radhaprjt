using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterCommandRepository
    {
        Task<UserManagement.Domain.Entities.MiscTypeMaster> CreateAsync(UserManagement.Domain.Entities.MiscTypeMaster miscTypeMaster);   
        Task<bool> UpdateAsync(int id, UserManagement.Domain.Entities.MiscTypeMaster miscTypeMaster);
        Task<bool> DeleteAsync(int id,UserManagement.Domain.Entities.MiscTypeMaster miscTypeMaster); 
    }
}