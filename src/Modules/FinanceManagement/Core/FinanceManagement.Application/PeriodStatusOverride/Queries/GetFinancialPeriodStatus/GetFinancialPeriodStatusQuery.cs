using FinanceManagement.Application.PeriodStatusOverride.Dto;
using MediatR;

namespace FinanceManagement.Application.PeriodStatusOverride.Queries.GetFinancialPeriodStatus
{
    public sealed record GetFinancialPeriodStatusQuery(int PeriodId)
        : IRequest<FinancialPeriodStatusDto?>;
}
