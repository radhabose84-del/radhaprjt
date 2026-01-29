using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterCommandRepository
    {
        
        Task<Core.Domain.Entities.MiscMaster> CreateAsync(Core.Domain.Entities.MiscMaster miscMaster);  

        Task<int> GetMaxSortOrderAsync();

        Task<bool> UpdateAsync(int id, Core.Domain.Entities.MiscMaster miscMaster);

        Task<bool> DeleteAsync(int id,Core.Domain.Entities.MiscMaster miscMaster); 
    }
}