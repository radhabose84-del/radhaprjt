using PurchaseManagement.Application.OCREntry.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IOCREntry
{
    public interface IOCREntryQueryRepository
    {
        Task<(List<OCREntryDto> Items, int Total)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<OCREntryDto?> GetByIdAsync(int id);
        Task<(List<OCREntryDto> Items, int Total)> GetPendingAsync(int pageNumber, int pageSize);
        /// <summary>
        /// Autocomplete list of OCRs. When <paramref name="showAll"/> is false (default), OCRs that are
        /// fully converted — a Raw Material PO exists and the total converted qty has reached the OCR
        /// quantity — are excluded. When true, every matching OCR is returned.
        /// </summary>
        Task<IReadOnlyList<OCREntryLookupDto>> AutocompleteAsync(string term, CancellationToken ct, bool approved = true, bool showAll = false);

        Task<bool> NotFoundAsync(int id);

        /// <summary>True when a non-deleted OCR with the same OcrDate (date part), ItemId and SupplierId already exists.</summary>
        Task<bool> DuplicateOcrExistsAsync(DateTimeOffset ocrDate, int itemId, int supplierId, int? excludeId = null);

        /// <summary>True when the OCR may still be edited/deleted (status is not Approved/Converted).</summary>
        Task<bool> IsEditableAsync(int id);

        /// <summary>True when the OCR is linked to a Raw Material PO (blocks delete — Rule #25).</summary>
        Task<bool> SoftDeleteValidationAsync(int id);

        // FK existence (same-module)
        Task<bool> MiscMasterExistsAsync(int id);
        Task<bool> PaymentTermExistsAsync(int id);
    }
}
