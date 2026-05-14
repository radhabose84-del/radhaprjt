using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;

namespace PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry
{
    public interface IRfqCommandRepository
    {
        Task<int> CreateAsync(RfqMaster rfq, CancellationToken ct = default);
        Task<RfqMaster?> GetAggregateTrackingAsync(int id, CancellationToken ct = default);
        Task<bool> IsDraftAsync(int id, CancellationToken ct = default);
        Task<string> GenerateNextCodeAsync( DateTimeOffset RfqDate,CancellationToken ct = default);
        Task<int> GetStatusIdByCodeAsync(string code, CancellationToken ct = default);
        Task UpdateAsync(int id, RfqMaster headerAfter, List<RfqItem> desiredItems, List<RfqSupplier> desiredSuppliers, CancellationToken ct = default);
        Task UpdateDraftPartialAsync(int id, RfqMaster headerAfter, List<RfqItem>? desiredItems, List<RfqSupplier>? desiredSuppliers, CancellationToken ct = default);
        Task<List<SupplierContacts>> GetSupplierContactsAsync(int rfqId, CancellationToken ct);
        Task<bool> RollbackStatusAsync(int id, CancellationToken ct = default);
        Task<string?> SoftDeleteAttachmentAsync(int rfqId, int attachmentId, CancellationToken ct = default);
    }
    public sealed record SupplierContacts(int Id, string? Email, string? Mobile);
}
