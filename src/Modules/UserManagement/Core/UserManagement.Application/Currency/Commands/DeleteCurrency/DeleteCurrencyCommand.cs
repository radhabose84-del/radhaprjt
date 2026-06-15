using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Currency.Commands.DeleteCurrency
{
    public class DeleteCurrencyCommand : IRequest<int>, IRequirePermission
    {
         public int Id { get; set; }
         public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
