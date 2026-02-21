#nullable disable

using MediatR;
using SalesManagement.Application.SalesSegment.Dto;

namespace SalesManagement.Application.SalesSegment.Queries.GetSalesSegmentAutoComplete
{
    public sealed record GetSalesSegmentAutoCompleteQuery(string Term) : IRequest<IReadOnlyList<SalesSegmentLookupDto>>;
}
