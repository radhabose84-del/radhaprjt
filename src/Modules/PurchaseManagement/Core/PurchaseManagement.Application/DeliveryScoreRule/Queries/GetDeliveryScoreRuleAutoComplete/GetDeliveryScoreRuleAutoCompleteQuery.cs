using Contracts.Dtos.Lookups.Purchase;
using MediatR;

namespace PurchaseManagement.Application.DeliveryScoreRule.Queries.GetDeliveryScoreRuleAutoComplete;

public sealed record GetDeliveryScoreRuleAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<DeliveryScoreRuleLookupDto>>;
