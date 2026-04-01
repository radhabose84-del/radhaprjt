using ProductionManagement.Application.ProductionPack.Dto;

namespace ProductionManagement.Application.Common.Interfaces.IProductionPack
{
    public interface IProductionQueryRepository
    {
        Task<(List<ProductionPackHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<ProductionPackHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<ProductionLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> UnitExistsAsync(int unitId);
        Task<bool> WarehouseExistsAsync(int warehouseId);
        Task<bool> BinExistsAsync(int binId);
        Task<bool> BinBelongsToWarehouseAsync(int binId, int warehouseId);
        Task<bool> LotExistsAsync(int lotId);
        Task<bool> PackTypeExistsAsync(int packTypeId);
        Task<bool> ItemExistsAsync(int itemId);
        Task<bool> QualityStatusExistsAsync(int qualityStatusId);
        Task<int> GetLastEndPackNoAsync(int productionYear);
        Task<bool> PackOverlapExistsAsync(int lotId, int startPackNo, int endPackNo, int? excludeDetailId = null);        
    }
}
