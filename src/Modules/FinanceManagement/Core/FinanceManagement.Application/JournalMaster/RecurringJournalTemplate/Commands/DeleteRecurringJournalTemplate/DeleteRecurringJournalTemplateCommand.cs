using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.DeleteRecurringJournalTemplate
{
    public sealed record DeleteRecurringJournalTemplateCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
