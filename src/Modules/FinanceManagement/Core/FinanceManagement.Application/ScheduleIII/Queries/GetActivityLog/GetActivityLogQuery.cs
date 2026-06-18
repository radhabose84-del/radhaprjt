using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetActivityLog
{
    public class GetActivityLogQuery : IRequest<ApiResponseDTO<List<ActivityLogDto>>>
    {
        public string? EntityName { get; set; }   // e.g. "ScheduleIIISectionItem" (optional filter)
        public int? EntityId { get; set; }         // specific row (optional filter)
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
