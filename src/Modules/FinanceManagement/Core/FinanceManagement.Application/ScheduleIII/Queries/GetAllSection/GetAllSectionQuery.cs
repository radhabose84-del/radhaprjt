using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetAllSection
{
    public class GetAllSectionQuery : IRequest<ApiResponseDTO<List<ScheduleIIISectionDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? ScheduleIIIMasterId { get; set; }
    }
}
