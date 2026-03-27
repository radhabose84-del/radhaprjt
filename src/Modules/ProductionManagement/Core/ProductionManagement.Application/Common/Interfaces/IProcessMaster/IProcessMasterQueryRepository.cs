using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.ProcessMaster.Dto;

namespace ProductionManagement.Application.Common.Interfaces.IProcessMaster
{
    public interface IProcessMasterQueryRepository
    {
        Task<(List<ProcessMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<ProcessMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<ProcessMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> ProcessNameExistsAsync(string processName, int? excludeId = null);
        Task<bool> NotFoundAsync(int id);
    }
}
