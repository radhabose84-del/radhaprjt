using ProductionManagement.Application.CountMaster.Dto;

namespace ProductionManagement.Application.Common.Interfaces.ICountMaster
{
    public interface ICountMasterQueryRepository
    {
        Task<(List<CountMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<CountMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<CountMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<string> GetNextCountCodeAsync();
        Task<bool> NotFoundAsync(int id);
        Task<bool> CountTypeExistsAsync(int id);
        Task<bool> CountCategoryExistsAsync(int id);
    }
}
