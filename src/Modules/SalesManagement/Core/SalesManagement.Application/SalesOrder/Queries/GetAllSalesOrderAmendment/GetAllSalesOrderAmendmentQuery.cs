using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Queries.GetAllSalesOrderAmendment
{
    public class GetAllSalesOrderAmendmentQuery : IRequest<ApiResponseDTO<List<SalesOrderAmendmentHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
