using MediatR;
using SalesManagement.Application.DeliveryChallan.Dto;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanAutoComplete
{
    public sealed record GetDeliveryChallanAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<DeliveryChallanLookupDto>>;
}
