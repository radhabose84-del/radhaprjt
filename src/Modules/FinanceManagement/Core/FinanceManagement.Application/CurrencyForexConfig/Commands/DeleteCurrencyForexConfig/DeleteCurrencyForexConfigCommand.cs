using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.CurrencyForexConfig.Commands.DeleteCurrencyForexConfig
{
    public sealed record DeleteCurrencyForexConfigCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
