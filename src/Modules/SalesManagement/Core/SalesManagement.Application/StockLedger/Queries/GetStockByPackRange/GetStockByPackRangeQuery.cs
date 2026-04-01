using Contracts.Common;
using MediatR;
using SalesManagement.Application.StockLedger.Dto;

namespace SalesManagement.Application.StockLedger.Queries.GetStockByPackRange
{
    public class GetStockByPackRangeQuery : IRequest<ApiResponseDTO<List<StockLedgerReportDto>>>
    {
        public int ItemId { get; set; }
        public int PackTypeId { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public int ProductionYear { get; set; }
    }
}
