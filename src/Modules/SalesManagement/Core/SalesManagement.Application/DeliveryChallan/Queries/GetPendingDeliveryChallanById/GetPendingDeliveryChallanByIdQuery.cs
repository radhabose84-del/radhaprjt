using MediatR;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetPendingDeliveryChallanById
{
    public class GetPendingDeliveryChallanByIdQuery : IRequest<PendingDeliveryChallanByIdDto?>
    {
        public int Id { get; set; }
    }
}
