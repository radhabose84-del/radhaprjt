using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterCommandRepository
    {

    Task<Core.Domain.Entities.MiscTypeMaster> CreateAsync(Core.Domain.Entities.MiscTypeMaster miscTypeMaster);   
    Task<bool> UpdateAsync(int id, Core.Domain.Entities.MiscTypeMaster miscTypeMaster);
    Task<bool> DeleteAsync(int id,Core.Domain.Entities.MiscTypeMaster miscTypeMaster); 
        
    }
}