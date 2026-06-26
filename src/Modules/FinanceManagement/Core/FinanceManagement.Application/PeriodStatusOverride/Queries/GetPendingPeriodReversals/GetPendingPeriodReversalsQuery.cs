using FinanceManagement.Application.PeriodStatusOverride.Dto;
using MediatR;

namespace FinanceManagement.Application.PeriodStatusOverride.Queries.GetPendingPeriodReversals
{
    public sealed record GetPendingPeriodReversalsQuery()
        : IRequest<IReadOnlyList<PeriodStatusOverrideDto>>;
}
