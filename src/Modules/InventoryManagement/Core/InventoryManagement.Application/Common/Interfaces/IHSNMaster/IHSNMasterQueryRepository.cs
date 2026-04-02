#nullable disable
using InventoryManagement.Application.HSNMaster.Queries.GetAllHSNMaster;
using InventoryManagement.Application.HSNMaster.Queries.GetHSNMasterAutoComplete;

namespace InventoryManagement.Application.Common.Interfaces.IHSNMaster
{
    public interface IHSNMasterQueryRepository
    {
        Task<(List<HSNMasterDto>, int)> GetAllAsync(int PageNumber, int PageSize, string SearchTerm);
        Task<HSNMasterDto> GetByIdAsync(int id);
        Task<List<GetHSNMasterAutoCompleteDto>> GetHSNMasterAutoCompleteAsync(string searchPattern = null, string typeCode = null);
        Task<bool> AlreadyExistsAsync(string hsnCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> FKColumnValidation(int id);        
        Task<bool> SoftDeleteValidation(int Id);
        Task<bool> IsHSNMasterLinkedAsync(int id);
    }
}