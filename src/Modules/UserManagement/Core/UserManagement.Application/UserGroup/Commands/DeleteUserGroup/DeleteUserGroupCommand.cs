using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.UserGroup.Commands.DeleteUserGroup
{
    public class DeleteUserGroupCommand :  IRequest<UserGroupDto>, IRequirePermission
       {
                public int Id { get; set; }                
                public PermissionType RequiredPermission => PermissionType.CanDelete;
       }
}
