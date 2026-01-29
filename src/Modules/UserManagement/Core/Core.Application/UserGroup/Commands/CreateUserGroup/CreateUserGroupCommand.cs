using Core.Application.Common.HttpResponse;
using Core.Application.UserGroup.Queries.GetUserGroup;
using MediatR;

namespace Core.Application.UserGroup.Commands.CreateUserGroup
{
    public class CreateUserGroupCommand : IRequest<UserGroupDto>
     {
          public string? GroupCode { get; set; }
          public string? GroupName { get; set; } 
     }         

}