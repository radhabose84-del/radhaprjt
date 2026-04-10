using Contracts.Dtos.Lookups.Inventory;
using InventoryManagement.Application.PriceGroupMaster.Dto;

namespace InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster
{
    public interface IPriceGroupMasterQueryRepository
    {
        Task<(List<PriceGroupMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);

        Task<PriceGroupMasterDto?> GetByIdAsync(int id);

        Task<IReadOnlyList<PriceGroupMasterLookupDto>> AutocompleteAsync(string term, CancellationToken cancellationToken);

        Task<bool> AlreadyExistsAsync(string priceGroupCode, int? id = null);

        Task<bool> NameAlreadyExistsAsync(string priceGroupName, int? id = null);

        Task<bool> NotFoundAsync(int id);
    }
}
