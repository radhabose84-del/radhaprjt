using SalesManagement.Application.Invoice.Dto;

namespace SalesManagement.Application.Common.Interfaces.IInvoice
{
    public interface IInvoiceQueryRepository
    {
        Task<(List<InvoiceHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<InvoiceHeaderDto?> GetByIdAsync(int id);
        Task<List<InvoiceHeaderDto>> GetByDispatchAdviceAsync(int dispatchAdviceId);
        Task<IReadOnlyList<InvoiceLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> DispatchAdviceExistsAsync(int dispatchAdviceId);
        Task<bool> IsAlreadyInvoicedAsync(int dispatchAdviceId);
        Task<bool> InvoiceTypeExistsAsync(int invoiceTypeId);
        Task<(int bags, decimal qty)> GetDispatchedQuantityAsync(int dispatchAdviceId, int itemId);
        Task<bool> IsCustomerTCSEnabledAsync(int partyId);
        Task<DateOnly> GetDispatchAdviceDateAsync(int dispatchAdviceId);
    }
}
