using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetAllLineItem
{
    public class GetAllLineItemQuery : IRequest<ApiResponseDTO<List<ScheduleIIISectionItemDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? ScheduleIIIMasterId { get; set; }
        public int? SectionId { get; set; }
    }
}
