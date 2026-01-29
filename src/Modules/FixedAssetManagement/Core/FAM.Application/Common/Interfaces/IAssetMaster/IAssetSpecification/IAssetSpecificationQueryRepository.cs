    using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationBasedMachineNo;
using FAM.Domain.Entities.AssetMaster;

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