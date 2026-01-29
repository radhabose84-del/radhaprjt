using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.FixedAsset;

namespace Contracts.Interfaces.External.IFixedAssetManagement
{
    public interface IAssetGroupGrpcClient
    {
        Task<AssetGroupDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<AssetGroupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}