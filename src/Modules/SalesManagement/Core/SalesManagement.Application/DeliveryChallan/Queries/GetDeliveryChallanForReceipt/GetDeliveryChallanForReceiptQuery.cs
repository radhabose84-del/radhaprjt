using MediatR;
using SalesManagement.Application.DeliveryChallan.Dto;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanForReceipt
{
    public sealed record GetDeliveryChallanForReceiptQuery(string Term)
        : IRequest<IReadOnlyList<DeliveryChallanLookupDto>>;
}
