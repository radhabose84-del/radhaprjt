using FinanceManagement.Application.FinancialYearMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Queries.GetPeriodForDate
{
    // CompanyId resolved from session inside the handler.
    public sealed record GetPeriodForDateQuery(DateOnly Date)
        : IRequest<FinancialPeriodMasterDto?>;
}
