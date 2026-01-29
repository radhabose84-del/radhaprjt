using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using FAM.Domain.Entities;

namespace FAM.Application.Common.Interfaces.IDepreciationGroup
{
    public interface IDepreciationGroupQueryRepository
    {
        Task<DepreciationGroupDTO>  GetByIdAsync(int depGroupId);
        Task<(List<DepreciationGroupDTO>,int)> GetAllDepreciationGroupAsync(int PageNumber, int PageSize, string? SearchTerm);        
        Task<List<DepreciationGroupDTO>> GetByDepreciationNameAsync(string depreciationGroupName);             
        Task<List<FAM.Domain.Entities.MiscMaster>> GetBookTypeAsync();             
        Task<List<FAM.Domain.Entities.MiscMaster>> GetDepreciationMethodAsync();        
        Task<bool> SoftDeleteValidation(int Id); 
    }
}