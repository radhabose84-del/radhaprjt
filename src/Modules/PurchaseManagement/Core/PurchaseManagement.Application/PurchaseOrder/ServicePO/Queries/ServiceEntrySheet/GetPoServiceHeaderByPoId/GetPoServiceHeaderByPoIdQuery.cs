using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetPoServiceHeaderByPoId
{
    public class GetPoServiceHeaderByPoIdQuery  : IRequest<PoServiceHeaderByIdDto?>
    {
        public int PoId { get; set; }
    }
}