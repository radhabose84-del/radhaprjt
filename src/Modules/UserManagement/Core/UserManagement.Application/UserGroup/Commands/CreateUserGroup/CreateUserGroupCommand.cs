using Contracts.Common;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using MediatR;

namespace UserManagement.Application.UserGroup.Commands.CreateUserGroup
{
    public class CreateUserGroupCommand : IRequest<UserGroupDto>
     {
          public string? GroupCode { get; set; }
          public string? GroupName { get; set; } 
     }         

}