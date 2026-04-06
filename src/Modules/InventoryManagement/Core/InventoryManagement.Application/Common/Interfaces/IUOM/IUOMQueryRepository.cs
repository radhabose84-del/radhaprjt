using InventoryManagement.Application.UOM.Queries.GetUOMs;
using InventoryManagement.Application.UOM.Queries.GetUOMTypeAutoComplete;

namespace InventoryManagement.Application.Common.Interfaces.IUOM
{
    public interface IUOMQueryRepository
    {
        Task<(List<InventoryManagement.Domain.Entities.UOM>, int)> GetAllUOMAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<InventoryManagement.Domain.Entities.UOM> GetByIdAsync(int id);
        Task<List<InventoryManagement.Domain.Entities.UOM>> GetUOM(string searchPattern);
        Task<List<UOMTypeAutoCompleteDto>> GetUOMType(string searchPattern);

        Task<InventoryManagement.Domain.Entities.UOM?> GetByUOMNameAsync(string name, int? id = null);

        Task<List<UOMDto>> GetUOMAsync();
        Task<bool> NotFoundAsync(int id);
        Task<bool> SoftDeleteValidation(int id);
        Task<bool> IsUOMLinkedAsync(int id);
    }
}