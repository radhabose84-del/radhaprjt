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
        Task<(List<InvoiceSearchDto>, int)> SearchInvoicesAsync(int partyId, string? searchTerm, bool lastOneYear, int pageNumber, int pageSize);
        Task<(List<PendingComplaintListDto>, int)> GetPendingComplaintsAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<string?> GetAttachmentFilePathAsync(int id);
        Task<IReadOnlyList<ComplaintForSalesReturnLookupDto>> GetComplaintsForSalesReturnAsync(string term, CancellationToken ct);
        Task<(List<PendingQCReviewListDto>, int)> GetPendingQCReviewAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<(List<PendingResolutionListDto>, int)> GetPendingResolutionAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<bool> IsReadyForResolutionAsync(int complaintHeaderId);

    }
}
