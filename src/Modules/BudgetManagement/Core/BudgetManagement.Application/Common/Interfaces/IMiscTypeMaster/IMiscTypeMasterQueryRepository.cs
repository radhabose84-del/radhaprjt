namespace BudgetManagement.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterQueryRepository
    {
        Task<(List<BudgetManagement.Domain.Entities.MiscTypeMaster>,int)> GetAllMiscTypeMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<BudgetManagement.Domain.Entities.MiscTypeMaster> GetByIdAsync(int id);

        Task<List<BudgetManagement.Domain.Entities.MiscTypeMaster>> GetMiscTypeMaster(string searchPattern);

        Task<BudgetManagement.Domain.Entities.MiscTypeMaster?> GetByMiscTypeMasterCodeAsync(string name,int? id = null);

        Task<bool> AlreadyExistsAsync(string miscTypeCode,int? id = null);

        Task<bool> NotFoundAsync(int Id );

        Task<bool> SoftDeleteValidation(int Id); 
    }
}