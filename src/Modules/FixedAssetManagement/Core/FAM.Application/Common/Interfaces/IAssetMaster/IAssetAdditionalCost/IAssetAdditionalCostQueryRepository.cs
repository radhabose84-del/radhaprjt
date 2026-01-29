using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetAdditionalCost
{
    public interface IAssetAdditionalCostQueryRepository
    {
         Task<List<FAM.Domain.Entities.MiscMaster>> GetCostTypeAsync();  
         Task<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost?> GetByIdAsync(int Id);
         Task<(List<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>,int)> GetAllAdditionalCostGroupAsync(int PageNumber, int PageSize, string? SearchTerm); 
    }
}