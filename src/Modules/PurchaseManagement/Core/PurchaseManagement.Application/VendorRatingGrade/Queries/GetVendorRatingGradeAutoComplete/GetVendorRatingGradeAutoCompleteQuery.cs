using Contracts.Dtos.Lookups.Purchase;
using MediatR;

namespace PurchaseManagement.Application.VendorRatingGrade.Queries.GetVendorRatingGradeAutoComplete;

public sealed record GetVendorRatingGradeAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<VendorRatingGradeLookupDto>>;
