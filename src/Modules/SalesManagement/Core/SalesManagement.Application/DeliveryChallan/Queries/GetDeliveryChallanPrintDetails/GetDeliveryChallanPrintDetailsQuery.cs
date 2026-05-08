using MediatR;
using SalesManagement.Application.DeliveryChallan.Dto;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanPrintDetails
{
    public sealed record GetDeliveryChallanPrintDetailsQuery(int Id) : IRequest<DeliveryChallanPrintDto?>;
}
