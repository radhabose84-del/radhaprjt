using SalesManagement.Application.SalesContact.Dto;

namespace SalesManagement.Application.Common.Interfaces.ISalesContact
{
    public interface ISalesContactQueryRepository
    {
        Task<(List<SalesContactDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesContactDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<SalesContactLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> MobileAlreadyExistsAsync(string mobileNumber, int? excludeId = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> ContactTypeExistsAsync(int contactTypeId);
    }
}
