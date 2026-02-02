
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBillEntry;

public interface IPurchaseBillEntryCommandRepository
{
    Task AddAsync(PurchaseBillEntryHeader entity, CancellationToken ct = default);
    Task UpdateAsync(PurchaseBillEntryHeader entity, CancellationToken ct = default);    
    Task DeleteByGrnIdAsync(int grnId, CancellationToken ct = default);
}
