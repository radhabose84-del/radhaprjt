using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterCommandRepository
    {
        Task<PurchaseManagement.Domain.Entities.MiscMaster> CreateAsync(PurchaseManagement.Domain.Entities.MiscMaster miscMaster);

        Task<int> GetMaxSortOrderAsync();

        Task<bool> UpdateAsync(int id, PurchaseManagement.Domain.Entities.MiscMaster miscMaster);

        Task<bool> DeleteAsync(int id, PurchaseManagement.Domain.Entities.MiscMaster miscMaster);  
        Task<Dictionary<int, PurchaseManagement.Domain.Entities.MiscMaster>> GetManyByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
    }
}