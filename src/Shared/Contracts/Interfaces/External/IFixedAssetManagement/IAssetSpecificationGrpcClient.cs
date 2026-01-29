using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Maintenance;

namespace Contracts.Interfaces.External.IFixedAssetManagement
{
    public interface IAssetSpecificationGrpcClient
    {
        Task<List<AssetSpecificationDto>> GetAllAssetSpecificationAsync();
    }
}