using Contracts.Dtos.Lookups.Purchase;
using PurchaseManagement.Application.VendorEvaluationCriteria.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria
{
    public interface IVendorEvaluationCriteriaQueryRepository
    {
        Task<(List<VendorEvaluationCriteriaDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<VendorEvaluationCriteriaDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<VendorEvaluationCriteriaLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string criteriaCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> ScoringMethodExistsAsync(int scoringMethodId);
        Task<bool> RatingImpactExistsAsync(int ratingImpactId);
    }
}
