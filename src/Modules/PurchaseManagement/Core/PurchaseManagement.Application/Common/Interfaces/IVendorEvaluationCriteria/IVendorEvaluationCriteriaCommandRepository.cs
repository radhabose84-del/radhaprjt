namespace PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria
{
    public interface IVendorEvaluationCriteriaCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.VendorEvaluation.VendorEvaluationCriteria entity);
        Task<int> UpdateAsync(Domain.Entities.VendorEvaluation.VendorEvaluationCriteria entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
