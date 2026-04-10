
using InventoryManagement.Application.ItemSpecificationMaster.Dto;

namespace InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster
{
    public interface IItemSpecificationMasterQueryRepository
    {
        Task<(List<ItemSpecificationMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<ItemSpecificationMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<ItemSpecificationMasterLookupDto>> AutocompleteAsync(string term, CancellationToken cancellationToken);
        Task<bool> AlreadyExistsAsync(string specificationCode, int? id = null);
        Task<bool> NameAlreadyExistsAsync(string specificationName, int? id = null);
        Task<bool> OrderAlreadyExistsAsync(int order, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsItemSpecificationMasterLinkedAsync(int id);
    }
}
