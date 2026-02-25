using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationBasedMachineNo;

namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification
{
    public interface IAssetSpecificationQueryRepository
    {
        Task<AssetSpecificationJsonDto> GetByIdAsync(int assetId);
        Task<(List<AssetSpecificationJsonDto>, int)> GetAllAssetSpecificationAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<AssetSpecificationJsonDto>> GetByAssetSpecificationNameAsync(string assetName);
        Task<bool> SoftDeleteValidation(int Id);
        Task<(List<AssetSpecBasedOnMachineNoDto>,int)> GetAssetSpecBasedOnMachineNos(int PageNumber, int PageSize, string? SearchTerm);   
    }
    }