using FAM.Application.UOM.Queries.GetUOMTypeAutoComplete;

namespace FAM.Application.Common.Interfaces.IUOM
{
    public interface IUOMQueryRepository
    {
        Task<(List<FAM.Domain.Entities.UOM>,int)> GetAllUOMAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<FAM.Domain.Entities.UOM> GetByIdAsync(int id);
        Task<List<FAM.Domain.Entities.UOM>> GetUOM(string searchPattern);
        Task<List<UOMTypeAutoCompleteDto>> GetUOMType(string searchPattern);

        Task<FAM.Domain.Entities.UOM?> GetByUOMNameAsync(string name,int? id = null);
        Task<bool> IsUomLinkedAsync(int uomId); // IsActive And Delete Validation 

    }
}