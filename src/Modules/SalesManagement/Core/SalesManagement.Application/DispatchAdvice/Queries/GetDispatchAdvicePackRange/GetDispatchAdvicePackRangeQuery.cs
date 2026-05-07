using MediatR;
using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackRange
{
    public class GetDispatchAdvicePackRangeQuery : IRequest<List<DispatchAdvicePackRangeDto>>
    {
        public int ItemId { get; set; }
        public int LotId { get; set; }
        public int PackTypeId { get; set; }
        public int Range { get; set; }
        // Optional — "FIFO" (default) orders by DocDate, PackNo ASC; "LIFO" orders by DocDate, PackNo DESC
        public string? OrderType { get; set; }
        // Optional — if null, UnitId from token (IP/Address service) is used to filter on UnitId; if provided, filter on SourceUnitId
        public int? SourceUnitId { get; set; }
        // Optional — MiscMaster Ids of defect statuses to include alongside 'Packed' stock.
        // Empty/null → Packed only (original behaviour). Any number of defect IDs may be passed
        // (e.g., DEFECT=176, DAMAGED=177, YARN MISMATCH=251 from MiscType=59).
        public List<int>? DefectStatusIds { get; set; }
    }
}
