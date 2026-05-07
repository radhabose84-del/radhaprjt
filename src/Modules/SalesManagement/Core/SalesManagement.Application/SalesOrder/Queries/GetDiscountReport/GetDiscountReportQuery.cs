using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Queries.GetDiscountReport
{
    public class GetDiscountReportQuery : IRequest<ApiResponseDTO<SalesOrderDiscountReportDto>>
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string? StatusName { get; set; }        // default = "Approved" (matches Sales.MiscMaster.Code)
        public int? PartyId { get; set; }              // optional filter
        public int? AgentId { get; set; }              // optional filter
        public int? SalesGroupId { get; set; }         // optional filter
        public string? DiscountSource { get; set; }    // optional: "SLAB" or "MD". Null = both.
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
