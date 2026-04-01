using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Queries.GetSalesOrderAmendmentById
{
    public class GetSalesOrderAmendmentByIdQuery : IRequest<ApiResponseDTO<SalesOrderAmendmentHeaderDto>>
    {
        public int Id { get; set; }
    }
}
