using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetSubTotals
{
    // Global catalog of sub-total formulas (Gross Profit / EBITDA / PBT / PAT).
    public class GetSubTotalsQuery : IRequest<ApiResponseDTO<List<ScheduleIIISubTotalDto>>>
    {
    }
}
