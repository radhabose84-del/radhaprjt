using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using FAM.Domain.Entities;

namespace FAM.Application.Common.Interfaces.ISpecificationMaster
{
    public interface ISpecificationMasterQueryRepository
    {
        Task<SpecificationMasters> GetByIdAsync(int specId);
        Task<(List<SpecificationMasterDTO>, int)> GetAllSpecificationGroupAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<SpecificationMasters>> GetBySpecificationNameAsync(int? assetGroupId, string specificationName);
        Task<bool> SoftDeleteValidation(int Id);
        Task<bool> IsSpecificationMasterLinkedAsync(int id);   
    }
}