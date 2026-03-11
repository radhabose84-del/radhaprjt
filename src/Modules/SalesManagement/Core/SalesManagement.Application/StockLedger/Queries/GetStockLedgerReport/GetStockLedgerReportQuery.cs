using Contracts.Common;
using MediatR;
using SalesManagement.Application.StockLedger.Dto;

namespace SalesManagement.Application.StockLedger.Queries.GetStockLedgerReport
{
    public class GetStockLedgerReportQuery : IRequest<ApiResponseDTO<List<StockLedgerReportDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int? ItemId { get; set; }
        public int? LotId { get; set; }
        public int? WarehouseId { get; set; }
        public int? BinId { get; set; }
        public int? StatusId { get; set; }
        public int? PackNo { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
    }
}
