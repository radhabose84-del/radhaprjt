using MediatR;

namespace SalesManagement.Application.Invoice.Queries.GetDispatchTrackingDetails
{
    public class GetDispatchTrackingDetailsQuery : IRequest<DispatchTrackingDetailsDto?>
    {
        public int SalesOrderId { get; set; }
    }
}
