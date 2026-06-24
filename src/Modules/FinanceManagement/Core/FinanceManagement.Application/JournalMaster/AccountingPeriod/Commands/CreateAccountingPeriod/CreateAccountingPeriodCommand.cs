using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.CreateAccountingPeriod
{
    public class CreateAccountingPeriodCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        // CompanyId is taken from the session token (IIPAddressService) — never from the payload.
        public int FinancialYearId { get; set; }
        public string? PeriodName { get; set; }
        public int PeriodNo { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int StatusId { get; set; }   // MiscMaster (PERIOD_STATUS): OPEN / CLOSED

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
