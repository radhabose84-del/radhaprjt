namespace PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade
{
    public interface IVendorRatingGradeCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.VendorEvaluation.VendorRatingGrade entity);
        Task<int> UpdateAsync(Domain.Entities.VendorEvaluation.VendorRatingGrade entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
