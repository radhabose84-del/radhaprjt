
using InventoryManagement.Application.ItemSpecificationValue.Dto;

namespace InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue
{
    public interface IItemSpecificationValueQueryRepository
    {
        Task<(List<ItemSpecificationValueDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<ItemSpecificationValueDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<ItemSpecificationValueLookupDto>> AutocompleteAsync(string term, CancellationToken cancellationToken);
        Task<bool> AlreadyExistsAsync(int specificationMasterId, string specificationValue, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SpecificationMasterExistsAsync(int specificationMasterId);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsItemSpecificationValueLinkedAsync(int id);
    }
}
