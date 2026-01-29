using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetDisposal
{
    public interface IAssetDisposalCommandRepository
    {
        Task<int> CreateAsync(AssetDisposal assetDisposal);
        Task<int> UpdateAsync(int id,AssetDisposal assetDisposal);
    }
}