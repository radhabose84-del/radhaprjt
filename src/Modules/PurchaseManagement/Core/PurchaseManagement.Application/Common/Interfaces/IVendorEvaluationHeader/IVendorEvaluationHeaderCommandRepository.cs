namespace PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader
{
    public interface IVendorEvaluationHeaderCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.VendorEvaluation.VendorEvaluationHeader entity);
        Task<int> UpdateAsync(Domain.Entities.VendorEvaluation.VendorEvaluationHeader entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
