using ProductionManagement.Application.YarnConversionHeader.Dto;

namespace ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader
{
    public interface IYarnConversionHeaderQueryRepository
    {
        Task<(List<YarnConversionHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<YarnConversionHeaderDto?> GetByIdAsync(int id);
        Task<List<YarnConversionHeaderLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> LotMasterExistsAsync(int id);
        Task<bool> PackTypeExistsAsync(int id);
        Task<bool> MiscMasterExistsAsync(int id);
    }
}
