using MediatR;
using PurchaseManagement.Application.Arrival.Dto;

namespace PurchaseManagement.Application.Arrival.Queries.GetArrivalById
{
    public class GetArrivalByIdQuery : IRequest<ArrivalDto?>
    {
        public int Id { get; set; }
    }
}
