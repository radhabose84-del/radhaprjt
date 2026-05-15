using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.ISalesAgreement
{
    public interface ISalesAgreementCommandRepository
    {
        Task<int> CreateAsync(SalesAgreementHeader entity, int transactionTypeId);

        // "Delete" semantics: sets StatusId to Cancelled (record is retained physically; audit fields auto-populated).
        Task<bool> CancelAsync(int id, int cancelledStatusId, CancellationToken ct);

        // Post-save rename of the uploaded Agent-PO attachment ({company}-{unit}-{agreementId}.{ext}).
        Task<bool> UpdateAgentPOAttachmentAsync(int id, string fileName, CancellationToken ct);
    }
}
