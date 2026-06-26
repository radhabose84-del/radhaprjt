using FinanceManagement.Application.FinancialYearMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialPeriodsForCompany
{
    // Task 5 / AC#5 — read endpoint for the future posting engine.
    // CompanyId resolved from {companyId} route segment in the controller.
    public sealed record GetFinancialPeriodsForCompanyQuery(int CompanyId)
        : IRequest<IReadOnlyList<FinancialPeriodMasterDto>>;
}
