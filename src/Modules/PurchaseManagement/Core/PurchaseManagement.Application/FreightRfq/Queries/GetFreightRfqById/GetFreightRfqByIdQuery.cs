using MediatR;
using PurchaseManagement.Application.FreightRfq.Dto;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqById
{
    public class GetFreightRfqByIdQuery : IRequest<FreightRfqDto?>
    {
        public int Id { get; set; }
    }
}
