using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.DeleteTaxAccountLinkage
{
    public sealed record DeleteTaxAccountLinkageCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
