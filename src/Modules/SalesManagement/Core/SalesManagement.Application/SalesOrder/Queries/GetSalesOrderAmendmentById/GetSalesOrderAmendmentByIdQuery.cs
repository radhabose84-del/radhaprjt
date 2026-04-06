using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Queries.GetSalesOrderAmendmentById
{
    public class GetSalesOrderAmendmentByIdQuery : IRequest<ApiResponseDTO<List<SalesOrderAmendmentHeaderDto>>>
    {
        public int SalesOrderHeaderId { get; set; }
    }
}
