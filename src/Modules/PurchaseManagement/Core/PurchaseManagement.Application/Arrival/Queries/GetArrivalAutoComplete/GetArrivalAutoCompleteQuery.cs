using MediatR;
using PurchaseManagement.Application.Arrival.Dto;

namespace PurchaseManagement.Application.Arrival.Queries.GetArrivalAutoComplete
{
    public sealed record GetArrivalAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<ArrivalLookupDto>>;
}
