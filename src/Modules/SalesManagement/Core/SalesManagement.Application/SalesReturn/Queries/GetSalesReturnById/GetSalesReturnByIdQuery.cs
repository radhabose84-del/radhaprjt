using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesReturn.Dto;

namespace SalesManagement.Application.SalesReturn.Queries.GetSalesReturnById
{
    public class GetSalesReturnByIdQuery : IRequest<ApiResponseDTO<SalesReturnHeaderDto>>
    {
        public int Id { get; set; }
    }
}
