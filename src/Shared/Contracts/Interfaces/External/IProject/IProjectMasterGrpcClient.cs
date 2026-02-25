using Contracts.Dtos.Project;

namespace Contracts.Interfaces.External.IProject
{
    public interface IProjectMasterGrpcClient
    {
        Task<ProjectMasterDto?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<List<ProjectMasterAutoCompleteDto>> GetAutoCompleteAsync( int? unitId, int? departmentId, string? searchTerm,int take = 10 ,CancellationToken ct = default);
    }
}