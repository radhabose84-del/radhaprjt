using FinanceManagement.Application.PeriodStatusOverride.Dto;
using MediatR;

namespace FinanceManagement.Application.PeriodStatusOverride.Queries.GetPeriodStatusHistory
{
    public sealed record GetPeriodStatusHistoryQuery(int PeriodId)
        : IRequest<IReadOnlyList<PeriodStatusOverrideDto>>;
}
