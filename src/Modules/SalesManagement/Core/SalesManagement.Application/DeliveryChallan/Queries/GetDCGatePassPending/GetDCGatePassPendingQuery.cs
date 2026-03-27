using MediatR;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetDCGatePassPending
{
    public sealed class GetDCGatePassPendingQuery
        : IRequest<List<GetDCGatePassPendingDto>>
    {
        public string? VehicleNo { get; set; }
    }
}
