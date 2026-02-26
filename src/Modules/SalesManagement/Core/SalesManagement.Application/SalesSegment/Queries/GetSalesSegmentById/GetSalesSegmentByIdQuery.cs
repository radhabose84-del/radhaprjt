using MediatR;
using SalesManagement.Application.SalesSegment.Dto;

namespace SalesManagement.Application.SalesSegment.Queries.GetSalesSegmentById
{
    public class GetSalesSegmentByIdQuery : IRequest<SalesSegmentDto?>
    {
        public int Id { get; set; }
    }
}
