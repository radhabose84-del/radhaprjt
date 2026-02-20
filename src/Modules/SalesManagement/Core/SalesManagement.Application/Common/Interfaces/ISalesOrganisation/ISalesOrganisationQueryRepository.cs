#nullable disable
using SalesManagement.Application.SalesOrganisation.Dto;

namespace SalesManagement.Application.Common.Interfaces.ISalesOrganisation
{
    public interface ISalesOrganisationQueryRepository
    {
        Task<(List<SalesOrganisationDto>, int)> GetAllAsync(int pageNumber, int pageSize, string searchTerm);
        Task<SalesOrganisationDto> GetByIdAsync(int id);
        Task<IReadOnlyList<SalesOrganisationLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string salesOrganisationCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> CompanyExistsAsync(int companyId);
    }
}
