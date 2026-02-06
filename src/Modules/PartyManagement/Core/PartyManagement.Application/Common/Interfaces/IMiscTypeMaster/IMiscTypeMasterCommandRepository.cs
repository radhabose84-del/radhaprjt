using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyManagement.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterCommandRepository
    {
        Task<PartyManagement.Domain.Entities.MiscTypeMaster> CreateAsync(PartyManagement.Domain.Entities.MiscTypeMaster miscTypeMaster);   
        Task<bool> UpdateAsync(int id, PartyManagement.Domain.Entities.MiscTypeMaster miscTypeMaster);
        Task<bool> DeleteAsync(int id,PartyManagement.Domain.Entities.MiscTypeMaster miscTypeMaster); 
    }
}