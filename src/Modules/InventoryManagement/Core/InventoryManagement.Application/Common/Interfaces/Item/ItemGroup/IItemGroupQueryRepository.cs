using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroupAutoComplete;

namespace InventoryManagement.Application.Common.Interfaces.Item.ItemGroup
{
    public interface IItemGroupQueryRepository
    {
        Task<ItemGroupDto> GetByIdAsync(int id);
        Task<(IEnumerable<dynamic>, int)> GetAllItemGroupAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<ItemGroupAutoCompleteDto>> GetItemGroupAutoCompleteAsync(string searchPattern);
        Task<bool> SoftDeleteValidation(int Id);
        Task<List<InventoryManagement.Domain.Entities.Item.ItemGroup>> GetAllItemGroupsAsync();       
            
    }
}