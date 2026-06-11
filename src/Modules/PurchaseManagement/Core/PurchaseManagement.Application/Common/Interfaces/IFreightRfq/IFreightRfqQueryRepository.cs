using PurchaseManagement.Application.FreightRfq.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IFreightRfq
{
    public interface IFreightRfqQueryRepository
    {
        // Grid (paginated + search + optional status filter). Names enriched via lookups.
        Task<(List<FreightRfqListDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? statusId);

        // Pending-approval list for the WorkFlow Approval screen (status = Pending). Paginated.
        Task<(List<FreightRfqListDto>, int)> GetPendingAsync(int pageNumber, int pageSize);

        // Full record (header + quotations + derived stats). Names enriched via lookups.
        Task<FreightRfqDto?> GetByIdAsync(int id);

        // Display-only peek of the next FreightRfqNumber (document sequence).
        Task<string> PeekNextNumberAsync(DateTimeOffset rfqDate);

        // Raw Material PO options for the "PO Reference" dropdown (PONumber + supplier from OCR).
        Task<IReadOnlyList<PoReferenceLookupDto>> GetRawMaterialPoReferencesAsync(string? term);

        // Auto-fill values for a selected Raw Material PO: supplier + source route (OCR) + Σ bale / Σ MT (details).
        Task<FreightRfqPoPrefillDto?> GetPoPrefillAsync(int rawMaterialPoId);

        // Existence (module NotFoundAsync convention: returns true when the RFQ exists, not deleted).
        Task<bool> NotFoundAsync(int id);

        // Current status code (Draft / Pending / Approved / Rejected) — guards edit/submit/approve transitions.
        Task<string?> GetStatusCodeAsync(int id);

        // Count of live quotation rows for the RFQ (R1).
        Task<int> GetQuotationCountAsync(int id);

        // True when the quotation row belongs to the RFQ and is live (submit selection validation).
        Task<bool> QuotationBelongsToRfqAsync(int rfqId, int quotationId);

        // True when the Purchase.MiscMaster id exists, is active and not deleted (FK validation for type/rate basis).
        Task<bool> MiscExistsAsync(int miscMasterId);

        // True when the PO exists, not deleted (FK validation for PO Reference).
        Task<bool> PurchaseOrderExistsAsync(int poId);
    }
}
