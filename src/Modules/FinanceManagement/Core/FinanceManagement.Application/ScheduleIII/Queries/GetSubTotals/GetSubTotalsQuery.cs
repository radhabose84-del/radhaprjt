using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetSubTotals
{
    public class GetSubTotalsQuery : IRequest<ApiResponseDTO<List<ScheduleIIISubTotalDto>>>
    {
        public int StructureId { get; set; }
    }
}
