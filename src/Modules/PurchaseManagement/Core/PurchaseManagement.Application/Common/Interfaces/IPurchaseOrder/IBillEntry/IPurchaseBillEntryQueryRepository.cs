
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBillEntry;

public interface IPurchaseBillEntryQueryRepository
{
    Task<PurchaseBillEntryHeader?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(IReadOnlyList<PurchaseBillEntryHeader> Rows, int Total)>
        GetListAsync(
            int? partyId,
            string? search,
            DateOnly? fromDate,
            DateOnly? toDate,
            int page,
            int size,
            CancellationToken ct = default);
    Task<bool> BillNumberExistsAsync(
        int partyId,
        string billNumber,
        int? excludeId,
        CancellationToken ct = default);
}
