using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterCommandRepository
    {

    Task<PurchaseManagement.Domain.Entities.MiscTypeMaster> CreateAsync(PurchaseManagement.Domain.Entities.MiscTypeMaster miscTypeMaster);   
    Task<bool> UpdateAsync(int id, PurchaseManagement.Domain.Entities.MiscTypeMaster miscTypeMaster);
    Task<bool> DeleteAsync(int id,PurchaseManagement.Domain.Entities.MiscTypeMaster miscTypeMaster); 
        
    }
}