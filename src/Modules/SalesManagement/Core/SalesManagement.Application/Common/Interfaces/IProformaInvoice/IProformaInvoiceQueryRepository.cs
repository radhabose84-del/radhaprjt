using SalesManagement.Application.ProformaInvoice.Dto;

namespace SalesManagement.Application.Common.Interfaces.IProformaInvoice
{
    public interface IProformaInvoiceQueryRepository
    {
        Task<(List<ProformaInvoiceDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<ProformaInvoiceDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<ProformaInvoiceLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<List<ProformaInvoiceDto>> GetBySalesOrderIdAsync(int salesOrderId);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SalesOrderExistsAndApprovedAsync(int salesOrderId);
        Task<bool> SalesOrderHasAdvancePaymentTypeAsync(int salesOrderId);
        Task<decimal> GetSalesOrderBalanceAsync(int salesOrderId);
        Task<bool> IsDraftStatusAsync(int id);
        Task<bool> StatusExistsAsync(int statusId);
        Task<decimal> GetProformaAmountAsync(int id);
        Task<bool> HasReceivedAdvancePaymentAsync(int salesOrderId);
        Task<ProformaInvoicePrintDto?> GetPrintDetailsAsync(int id);
    }
}
