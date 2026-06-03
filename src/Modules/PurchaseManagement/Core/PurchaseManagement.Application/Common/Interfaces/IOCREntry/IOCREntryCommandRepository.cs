using PurchaseManagement.Application.OCREntry.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IOCREntry
{
    public interface IOCREntryCommandRepository
    {
        Task<Domain.Entities.OCREntry> CreateAsync(Domain.Entities.OCREntry entity, int transactionTypeId, CancellationToken ct);
        Task<int> UpdateAsync(Domain.Entities.OCREntry entity, CancellationToken ct);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);

        // Workflow integration
        Task<bool> UpdateOcrApproveAsync(int id, int statusId, CancellationToken ct);
        Task<OCREntryWorkFlowDto> GetByIdOCRWorkFlowAsync(int id);
    }
}
