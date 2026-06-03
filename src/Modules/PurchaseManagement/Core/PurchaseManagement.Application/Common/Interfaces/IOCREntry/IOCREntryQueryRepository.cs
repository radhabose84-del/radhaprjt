using PurchaseManagement.Application.OCREntry.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IOCREntry
{
    public interface IOCREntryQueryRepository
    {
        Task<(List<OCREntryDto> Items, int Total)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<OCREntryDto?> GetByIdAsync(int id);
        Task<(List<OCREntryDto> Items, int Total)> GetPendingAsync(int pageNumber, int pageSize);
        Task<IReadOnlyList<OCREntryLookupDto>> AutocompleteAsync(string term, CancellationToken ct);

        Task<bool> NotFoundAsync(int id);

        /// <summary>True when the OCR may still be edited/deleted (status is not Approved/Converted).</summary>
        Task<bool> IsEditableAsync(int id);

        // FK existence (same-module)
        Task<bool> MiscMasterExistsAsync(int id);
        Task<bool> PaymentTermExistsAsync(int id);
    }
}
