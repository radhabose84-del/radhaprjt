using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetWbsLookup;

namespace ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure
{
    public interface IProjectWorkBreakdownStructureQueryRepository
    {

        Task<(IReadOnlyList<ProjectWorkBreakdownStructureDto> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<IReadOnlyList<ProjectWorkBreakdownStructureDto>> GetByProjectAsync(int projectId);
        Task<ProjectWorkBreakdownStructureDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<ProjectWorkBreakdownStructureAutocompleteDto>> GetAutocompleteAsync(int? projectId, string? searchPattern);
        Task<bool> IsNameUniqueAsync(int projectId, string wbsName, int? excludeId = null);
        Task<int> GetParentLevelAsync(int parentId);
        
        Task<List<ProjectWbsLookupDto>> GetWbsLookupAsync( int? projectId = null, CancellationToken ct = default);
    }
}