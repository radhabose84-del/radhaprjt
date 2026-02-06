using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterCommandRepository
    {
          Task<PartyManagement.Domain.Entities.MiscMaster> CreateAsync(PartyManagement.Domain.Entities.MiscMaster miscMaster);  

          Task<int> GetMaxSortOrderAsync();

          Task<bool> UpdateAsync(int id, PartyManagement.Domain.Entities.MiscMaster miscMaster);

          Task<bool> DeleteAsync(int id,PartyManagement.Domain.Entities.MiscMaster miscMaster);  
    }
}