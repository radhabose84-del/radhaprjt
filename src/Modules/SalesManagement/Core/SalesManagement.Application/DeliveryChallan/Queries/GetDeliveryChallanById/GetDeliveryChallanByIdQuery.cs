using MediatR;
using SalesManagement.Application.DeliveryChallan.Dto;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanById
{
    public class GetDeliveryChallanByIdQuery : IRequest<DeliveryChallanHeaderDto?>
    {
        public int Id { get; set; }
    }
}
