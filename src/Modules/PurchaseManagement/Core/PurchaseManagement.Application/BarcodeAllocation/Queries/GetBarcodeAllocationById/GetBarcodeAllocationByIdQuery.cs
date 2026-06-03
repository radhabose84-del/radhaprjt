using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocationById
{
    public class GetBarcodeAllocationByIdQuery : IRequest<BarcodeAllocationDto?>
    {
        public int Id { get; set; }
    }
}
