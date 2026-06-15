using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.DocumentSequence.Commands.DeleteDocumentSequence
{
    public sealed record DeleteDocumentSequenceCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
