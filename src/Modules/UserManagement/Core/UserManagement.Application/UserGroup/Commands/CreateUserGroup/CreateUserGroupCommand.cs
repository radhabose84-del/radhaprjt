using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.UserGroup.Commands.CreateUserGroup
{
    public class CreateUserGroupCommand : IRequest<UserGroupDto>, IRequirePermission
     {
          public string? GroupCode { get; set; }
          public string? GroupName { get; set; } 
          public PermissionType RequiredPermission => PermissionType.CanAdd;
     }         

}
