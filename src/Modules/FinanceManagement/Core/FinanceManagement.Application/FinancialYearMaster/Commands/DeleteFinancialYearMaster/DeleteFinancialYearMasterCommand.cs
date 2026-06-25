using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Commands.DeleteFinancialYearMaster
{
    public sealed record DeleteFinancialYearMasterCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
