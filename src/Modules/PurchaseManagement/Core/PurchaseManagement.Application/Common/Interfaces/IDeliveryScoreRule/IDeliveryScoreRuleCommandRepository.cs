namespace PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule
{
    public interface IDeliveryScoreRuleCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.VendorEvaluation.DeliveryScoreRule entity);
        Task<int> UpdateAsync(Domain.Entities.VendorEvaluation.DeliveryScoreRule entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
