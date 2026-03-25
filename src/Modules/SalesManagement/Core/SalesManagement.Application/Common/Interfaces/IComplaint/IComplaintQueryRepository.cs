using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Common.Interfaces.IComplaint
{
    public interface IComplaintQueryRepository
    {
        Task<(List<ComplaintHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<ComplaintHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<ComplaintLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> CustomerExistsAsync(int customerId);
        Task<bool> InvoiceBelongsToCustomerAsync(int invoiceHeaderId, int customerId);
        Task<List<CustomerInvoiceDto>> GetCustomerInvoicesAsync(int customerId);
        Task<List<InvoiceLineDetailDto>> GetInvoiceLineDetailsAsync(int invoiceHeaderId);
    }
}
