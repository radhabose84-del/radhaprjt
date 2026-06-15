using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.YarnType.Dto;

namespace ProductionManagement.Application.Common.Interfaces.IYarnType
{
    public interface IYarnTypeQueryRepository
    {
        Task<(List<YarnTypeDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<YarnTypeDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<YarnTypeLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string yarnTypeCode, int? id = null);
        Task<bool> YarnTypeNameExistsAsync(string yarnTypeName, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> CurrencyExistsAsync(int currencyId, CancellationToken ct = default);
    }
}
