using MediatR;
using Contracts.Common;

namespace UserManagement.Application.CustomFields.Commands.DeleteCustomField
{
    public class DeleteCustomFieldCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
