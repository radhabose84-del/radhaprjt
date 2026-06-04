using PurchaseManagement.Application.OCREntry.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IOCREntry
{
    public interface IOCREntryCommandRepository
    {
        Task<Domain.Entities.OCREntry> CreateAsync(Domain.Entities.OCREntry entity, int transactionTypeId, CancellationToken ct);
        Task<int> UpdateAsync(Domain.Entities.OCREntry entity, CancellationToken ct);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);

        /// <summary>
        /// Clears the DocumentPath on the OCR row (if any) that references the given file name.
        /// Returns true when a row was updated; false when no OCR references the file (e.g. an
        /// unsaved temp upload).
        /// </summary>
        Task<bool> ClearDocumentPathByFileNameAsync(string fileName, CancellationToken ct);

        // Workflow integration
        Task<bool> UpdateOcrApproveAsync(int id, int statusId, CancellationToken ct);
        Task<OCREntryWorkFlowDto> GetByIdOCRWorkFlowAsync(int id);
    }
}
