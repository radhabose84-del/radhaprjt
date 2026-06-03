namespace PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation
{
    public interface IBarcodeAllocationCommandRepository
    {
        // Generates AllocationNumber + default status, persists, then recomputes the parent series. Returns new Id.
        Task<int> CreateAsync(PurchaseManagement.Domain.Entities.BarcodeAllocation entity);

        // Updates mutable fields (AllocationNumber immutable) and recomputes affected series. Returns the affected Id.
        Task<int> UpdateAsync(PurchaseManagement.Domain.Entities.BarcodeAllocation entity);

        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
