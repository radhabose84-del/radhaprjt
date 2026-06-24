using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.DeleteAccountingPeriod
{
    public sealed record DeleteAccountingPeriodCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
