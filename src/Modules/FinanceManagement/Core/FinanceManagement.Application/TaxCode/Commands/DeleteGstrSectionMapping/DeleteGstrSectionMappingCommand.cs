using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.DeleteGstrSectionMapping
{
    public sealed record DeleteGstrSectionMappingCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
