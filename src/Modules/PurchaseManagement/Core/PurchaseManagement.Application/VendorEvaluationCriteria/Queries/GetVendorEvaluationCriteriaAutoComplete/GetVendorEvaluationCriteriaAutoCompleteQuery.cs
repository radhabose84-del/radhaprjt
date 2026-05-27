using Contracts.Dtos.Lookups.Purchase;
using MediatR;

namespace PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetVendorEvaluationCriteriaAutoComplete;

public sealed record GetVendorEvaluationCriteriaAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<VendorEvaluationCriteriaLookupDto>>;
