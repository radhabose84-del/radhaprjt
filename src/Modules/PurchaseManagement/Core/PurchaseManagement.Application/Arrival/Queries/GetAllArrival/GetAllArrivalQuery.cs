using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Arrival.Dto;

namespace PurchaseManagement.Application.Arrival.Queries.GetAllArrival
{
    public class GetAllArrivalQuery : IRequest<ApiResponseDTO<List<ArrivalDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }

        // null = all, true = pending QC (QcStatusId null), false = QC signed off (QcStatusId set)
        public bool? PendingStatus { get; set; }

        // Explicit QC status filter (QcStatusId). null = no filter.
        public int? StatusId { get; set; }

        // ArrivalDate range filter (inclusive). null = no bound.
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}
