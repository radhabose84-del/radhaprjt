using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.StockLedger.Queries.GetStockByPackRange
{
    public class GetStockByPackRangeQuery : IRequest<ApiResponseDTO<object>>
    {
        public int? ItemId { get; set; }
        public int? PackTypeId { get; set; }
        public int? StartPackNo { get; set; }
        public int? EndPackNo { get; set; }
        public int ProductionYear { get; set; }
    }
}
