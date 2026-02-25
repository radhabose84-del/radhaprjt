
using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesSegment.Dto;

namespace SalesManagement.Application.SalesSegment.Queries.GetSalesSegmentById
{
    public class GetSalesSegmentByIdQuery : IRequest<ApiResponseDTO<SalesSegmentDto>>
    {
        public int Id { get; set; }
    }
}
