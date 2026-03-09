using MediatR;
using SalesManagement.Application.StoReceipt.Dto;

namespace SalesManagement.Application.StoReceipt.Queries.GetDcOpenQty
{
    public class GetDcOpenQtyQuery : IRequest<DcOpenQtyDto?>
    {
        public int DcDetailId { get; set; }
    }
}
