using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Application.Invoice.Queries.GetDispatchTrackingDetails;
using SalesManagement.Application.Invoice.Queries.GetInvoiceGatePassPending;
using SalesManagement.Application.Invoice.Queries.GetInvoicePending;

namespace SalesManagement.Application.Common.Interfaces.IInvoice
{
    public interface IInvoiceQueryRepository
    {
        Task<(List<InvoiceHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, string? status = null);
        Task<(List<GetInvoicePendingDto>, int)> GetInvoicePendingAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<List<GetInvoiceGatePassPendingDto>> GetInvoiceGatePassPendingAsync(string? vehicleNo);
        Task<InvoiceHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<InvoiceLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> DispatchAdviceExistsAsync(int dispatchAdviceId);
        Task<bool> MiscMasterExistsAsync(int id);
        Task<bool> IsAlreadyInvoicedAsync(int dispatchAdviceId);

        Task<(int bags, decimal qty)> GetDispatchedQuantityAsync(int dispatchAdviceId, int itemId);
        Task<bool> IsCustomerTCSEnabledAsync(int partyId);
        Task<DateOnly> GetDispatchAdviceDateAsync(int dispatchAdviceId);
        Task<bool> IsInvoicePendingAsync(int invoiceId);
        Task<InvoicePrintDto?> GetPrintDetailsAsync(int id);
        Task<InvoiceForEInvoiceDto?> GetInvoiceForEInvoiceAsync(int invoiceId);
        Task<DispatchTrackingDetailsDto?> GetDispatchTrackingDetailsAsync(int salesOrderId, CancellationToken ct);
    }
}
