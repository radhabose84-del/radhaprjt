using PurchaseManagement.Application.RawMaterialPO.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO
{
    public interface IRawMaterialPOQueryRepository
    {
        Task<(List<RawMaterialPODto> Items, int Total)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<RawMaterialPODto?> GetByIdAsync(int id);
        /// <summary>
        /// Autocomplete list of Raw Material POs. When <paramref name="showAll"/> is false (default),
        /// POs that are fully arrived — an arrival exists and total arrived qty has reached the PO
        /// ordered qty — are excluded. When true, every active PO is returned.
        /// </summary>
        Task<IReadOnlyList<RawMaterialPOLookupDto>> AutocompleteAsync(string term, bool showAll, CancellationToken ct);

        Task<bool> NotFoundAsync(int id);

        // ── OCR conversion helpers ──
        /// <summary>True when the OCR exists, is not deleted, and its status is Approved.</summary>
        Task<bool> OcrExistsAndApprovedAsync(int ocrId);
        Task<decimal> GetOcrQuantityAsync(int ocrId);
        /// <summary>Sum of converted bales for the OCR across non-deleted Raw Material POs (optionally excluding one header).</summary>
        Task<decimal> GetConvertedQuantityAsync(int ocrId, int? excludeHeaderId);

        // ── FK existence (same-module) ──
        Task<bool> MiscMasterExistsAsync(int id);
    }
}
