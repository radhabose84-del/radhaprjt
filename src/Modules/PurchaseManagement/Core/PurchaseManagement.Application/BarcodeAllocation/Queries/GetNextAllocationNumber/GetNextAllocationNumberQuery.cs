using MediatR;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetNextAllocationNumber
{
    public class GetNextAllocationNumberQuery : IRequest<string>
    {
        public DateTimeOffset AllocationDate { get; set; }
    }
}
