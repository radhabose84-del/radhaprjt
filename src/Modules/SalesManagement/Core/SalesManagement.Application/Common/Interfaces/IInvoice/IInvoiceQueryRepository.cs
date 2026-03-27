using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Application.Invoice.Queries.GetInvoiceGatePassPending;
using SalesManagement.Application.Invoice.Queries.GetInvoicePending;

namespace SalesManagement.Application.Common.Interfaces.IInvoice
{
    public interface IInvoiceQueryRepository
    {
        Task<(List<InvoiceHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<(List<GetInvoicePendingDto>, int)> GetInvoicePendingAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<List<GetInvoiceGatePassPendingDto>> GetInvoiceGatePassPendingAsync();
        Task<InvoiceHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<InvoiceLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> DispatchAdviceExistsAsync(int dispatchAdviceId);
        Task<bool> IsAlreadyInvoicedAsync(int dispatchAdviceId);
        Task<bool> InvoiceTypeExistsAsync(int invoiceTypeId);
        Task<(int bags, decimal qty)> GetDispatchedQuantityAsync(int dispatchAdviceId, int itemId);
        Task<bool> IsCustomerTCSEnabledAsync(int partyId);
        Task<DateOnly> GetDispatchAdviceDateAsync(int dispatchAdviceId);
        Task<bool> IsInvoicePendingAsync(int invoiceId);
    }
}
