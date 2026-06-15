using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Currency.Commands.CreateCurrency
{
    public class CreateCurrencyCommand :IRequest<int>, IRequirePermission
    { 
        public string? Code { get; set; }
        public string? Name { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
