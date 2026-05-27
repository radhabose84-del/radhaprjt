using Contracts.Dtos.Lookups.Purchase;
using PurchaseManagement.Application.DeliveryScoreRule.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule
{
    public interface IDeliveryScoreRuleQueryRepository
    {
        Task<(List<DeliveryScoreRuleDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<DeliveryScoreRuleDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<DeliveryScoreRuleLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string ruleCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
    }
}
