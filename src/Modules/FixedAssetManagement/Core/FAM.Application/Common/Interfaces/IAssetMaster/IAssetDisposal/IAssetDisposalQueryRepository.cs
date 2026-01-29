using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetDisposal
{
    public interface IAssetDisposalQueryRepository
    {
         Task<List<FAM.Domain.Entities.MiscMaster>> GetDisposalType(); 
         Task<AssetDisposal?> GetByIdAsync(int Id);
         Task<(List<AssetDisposal>,int)> GetAllAssetDisposalAsync(int PageNumber, int PageSize, string? SearchTerm); 
    }
}