
namespace BudgetManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterQueryRepository
    {
        Task<(List<BudgetManagement.Domain.Entities.MiscMaster>, int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<BudgetManagement.Domain.Entities.MiscMaster> GetByIdAsync(int id);
        Task<List<BudgetManagement.Domain.Entities.MiscMaster>> GetMiscMaster(string searchPattern, string miscTypeCode);
        Task<BudgetManagement.Domain.Entities.MiscMaster?> GetByMiscMasterCodeAsync(string name, int? id = null);

        // Task<bool> AlreadyExistsAsync(string code,int? id = null);
        Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> FKColumnValidation(int ShiftMasterId);
        Task<BudgetManagement.Domain.Entities.MiscMaster> GetMiscMasterByName(string miscTypeCode, string miscTypeName);
        Task<Domain.Entities.MiscMaster?> GetByTypeAndCodeAsync(string miscTypeCode, string code, CancellationToken ct = default);
    }
}