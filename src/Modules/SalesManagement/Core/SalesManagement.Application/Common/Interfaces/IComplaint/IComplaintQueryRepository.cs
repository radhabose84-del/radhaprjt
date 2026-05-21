using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Common.Interfaces.IComplaint
{
    public interface IComplaintQueryRepository
    {
        Task<(List<ComplaintHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, string? statusFilter);
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

        // Returns true if the complaint's current status is terminal/finalized
        // (QC Accepted or Closed). Used by UpdateComplaintCommandValidator to
        // block edits on records that should no longer be mutable.
        Task<bool> IsComplaintFinalizedAsync(int id);

        // Resolves the distinct effective Agent id(s) for a complaint (Sales schema only):
        // ComplaintHeader → ComplaintDetail → InvoiceHeader → DispatchAdviceHeader
        // → SalesOrderHeader, agent = COALESCE(NULLIF(InvoiceHeader.AgentId,0),
        // SalesOrderHeader.AgentId). Returns distinct agent ids > 0; empty if none.
        // The Agent→Marketing-Officer→UserId resolution is done separately via the
        // cross-module IOfficerAgentUserLookup (no cross-module SQL JOIN here).
        Task<List<int>> GetComplaintAgentIdsAsync(int complaintId);
    }
}
