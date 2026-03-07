using MediatR;
using SalesManagement.Application.DeliveryChallan.Dto;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetStoOpenQty
{
    public class GetStoOpenQtyQuery : IRequest<StoOpenQtyDto?>
    {
        public int StoDetailId { get; set; }
    }
}
