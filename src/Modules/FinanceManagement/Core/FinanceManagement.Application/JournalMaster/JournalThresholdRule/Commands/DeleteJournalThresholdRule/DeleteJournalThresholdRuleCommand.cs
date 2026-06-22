using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.DeleteJournalThresholdRule
{
    public sealed record DeleteJournalThresholdRuleCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
