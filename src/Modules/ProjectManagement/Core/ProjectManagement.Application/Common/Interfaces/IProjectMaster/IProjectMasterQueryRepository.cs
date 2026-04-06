using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectPendingApprovals;
using ProjectManagement.Application.ProjectMaster.Queries.ProjectMasterAutoComplete;

namespace ProjectManagement.Application.Common.Interfaces.IProjectMaster
{
    public interface IProjectMasterQueryRepository
    {
        Task<(IReadOnlyList<GetProjectMasterDto> Items, int TotalCount)> GetProjectmasterAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            CancellationToken ct = default);

        Task<GetProjectMasterDto?> GetByIdAsync(int id, CancellationToken ct = default);

        // Task<List<ProjectMasterAutoCompleteDto>> GetProjectMasterAutoCompleteAsync(  int? unitId,   int? departmentId,   string? searchTerm,  CancellationToken ct = default);

        Task<List<ProjectMasterAutoCompleteDto>> GetProjectMasterAutoCompleteAsync(int? unitId, int? departmentId, string? searchTerm, int take, CancellationToken ct = default);

        Task<string?> GetProjectNameAsync(int projectId, CancellationToken ct = default);
       
         Task<(List<GetProjectPendingApprovalDto> Rows, int TotalCount)> GetProjectPendingAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            int? projectId,
            int? departmentId,
            int? projectTypeId,
            int? budgetYearId,
            int unitId,
            int pendingStatusId,
            CancellationToken ct = default);

        Task<bool> NotFoundAsync(int id);

        Task<bool> SoftDeleteValidationAsync(int id);
    }
}