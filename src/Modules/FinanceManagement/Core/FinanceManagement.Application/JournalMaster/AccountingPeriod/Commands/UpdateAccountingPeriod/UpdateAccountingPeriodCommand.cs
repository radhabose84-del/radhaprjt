using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.UpdateAccountingPeriod
{
    public class UpdateAccountingPeriodCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        // FinancialYearId and PeriodNo are immutable — not included.
        public string? PeriodName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int StatusId { get; set; }   // MiscMaster (PERIOD_STATUS): OPEN / CLOSED
        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
