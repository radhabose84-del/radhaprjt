using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.DeleteTaxCodeMaster
{
    public sealed record DeleteTaxCodeMasterCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
