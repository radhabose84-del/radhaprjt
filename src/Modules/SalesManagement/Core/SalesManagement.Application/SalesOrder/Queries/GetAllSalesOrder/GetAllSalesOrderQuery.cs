using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Queries.GetAllSalesOrder
{
    public class GetAllSalesOrderQuery : IRequest<ApiResponseDTO<List<SalesOrderHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public DateOnly? OrderDateFrom { get; set; }
        public DateOnly? OrderDateTo { get; set; }
        public string? PartyName { get; set; }
        public string? StatusName { get; set; }
        public int? SalesOrderTypeMasterId { get; set; }
    }
}
