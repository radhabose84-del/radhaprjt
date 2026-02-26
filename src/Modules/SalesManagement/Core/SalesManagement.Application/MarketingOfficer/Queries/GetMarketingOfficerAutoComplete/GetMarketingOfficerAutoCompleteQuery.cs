using MediatR;
using SalesManagement.Application.MarketingOfficer.Dto;

namespace SalesManagement.Application.MarketingOfficer.Queries.GetMarketingOfficerAutoComplete
{
    public sealed record GetMarketingOfficerAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<MarketingOfficerLookupDto>>;
}
