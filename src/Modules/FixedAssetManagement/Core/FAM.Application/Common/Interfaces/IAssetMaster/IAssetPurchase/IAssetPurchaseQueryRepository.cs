using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase
{
    public interface IAssetPurchaseQueryRepository
    {
        Task<List<FAM.Domain.Entities.AssetSource>> GetAssetSources(string searchPattern);
        // GetAssetUnit removed - use IUnitLookup.GetUserUnitByUserNameAsync instead
        Task<List<FAM.Domain.Entities.AssetPurchase.AssetGrn>> GetAssetGrnNo(string OldUnitId,int AssetSourceId, string? SearchTerm);
        Task<List<FAM.Domain.Entities.AssetPurchase.AssetGrnItem>> GetAssetGrnItem(string OldUnitId, int AssetSourceId,int GrnNo);
        Task<List<FAM.Domain.Entities.AssetPurchase.AssetGrnDetails>> GetAssetGrnItemDetails(string OldUnitId,int AssetSourceId ,int GrnNo,int GrnSerialNo);
        Task<FAM.Domain.Entities.AssetPurchase.AssetPurchaseDetails?> GetByIdAsync(int Id);
        Task<(List<FAM.Domain.Entities.AssetPurchase.AssetPurchaseDetails>,int)> GetAllPurchaseDetails(int PageNumber, int PageSize, string? SearchTerm);
    }
    
}