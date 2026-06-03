using MediatR;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetNextAllocationFrom
{
    public class GetNextAllocationFromQuery : IRequest<long>
    {
        public int BarcodeSeriesId { get; set; }
    }
}
