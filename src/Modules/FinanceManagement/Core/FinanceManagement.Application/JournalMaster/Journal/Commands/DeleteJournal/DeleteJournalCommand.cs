using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.DeleteJournal
{
    public sealed record DeleteJournalCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
