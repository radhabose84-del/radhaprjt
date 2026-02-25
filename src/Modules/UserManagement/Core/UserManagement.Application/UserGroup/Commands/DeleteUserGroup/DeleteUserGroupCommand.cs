using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using MediatR;

namespace UserManagement.Application.UserGroup.Commands.DeleteUserGroup
{
    public class DeleteUserGroupCommand :  IRequest<UserGroupDto>
       {
                public int Id { get; set; }                
       }
}