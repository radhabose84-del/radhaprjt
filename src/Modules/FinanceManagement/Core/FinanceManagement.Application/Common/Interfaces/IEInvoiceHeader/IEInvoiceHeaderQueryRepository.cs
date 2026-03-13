using FinanceManagement.Application.EInvoiceHeader.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader
{
    public interface IEInvoiceHeaderQueryRepository
    {
        Task<(List<EInvoiceHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<EInvoiceHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<EInvoiceHeaderLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> IrnNumberExistsAsync(string irnNumber, int? excludeId = null);
        Task<bool> NotFoundAsync(int id);
    }
}
