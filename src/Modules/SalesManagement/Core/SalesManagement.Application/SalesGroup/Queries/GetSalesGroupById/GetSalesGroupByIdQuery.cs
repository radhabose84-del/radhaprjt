using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesGroup.Dto;

namespace SalesManagement.Application.SalesGroup.Queries.GetSalesGroupById
{
    public class GetSalesGroupByIdQuery : IRequest<ApiResponseDTO<SalesGroupDto>>
    {
        public int Id { get; set; }
    }
}
