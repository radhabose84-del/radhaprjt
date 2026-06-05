using PurchaseManagement.Domain.Entities.RawMaterialPO;

namespace PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO
{
    public interface IRawMaterialPOCommandRepository
    {
        Task<int> CreateAsync(RawMaterialPOHeader entity, int transactionTypeId, CancellationToken ct);
        Task<int> UpdateAsync(RawMaterialPOHeader entity, CancellationToken ct);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
