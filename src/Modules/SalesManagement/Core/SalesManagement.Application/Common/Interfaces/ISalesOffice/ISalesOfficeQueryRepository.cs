using SalesManagement.Application.SalesOffice.Dto;

namespace SalesManagement.Application.Common.Interfaces.ISalesOffice
{
    public interface ISalesOfficeQueryRepository
    {
        Task<(List<SalesOfficeDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesOfficeDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<SalesOfficeLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string salesOfficeName, int salesOrganisationId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SalesOrganisationExistsAsync(int salesOrganisationId);
        Task<bool> CityExistsAsync(int cityId);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsSalesOfficeLinkedAsync(int id);
    }
}
