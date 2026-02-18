using Contracts.Common;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using MediatR;

namespace UserManagement.Application.UserGroup.Queries.GetUserGroupById
{
    public class GetUserGroupByIdQuery : IRequest<UserGroupDto>
    {
        public int Id { get; set; }
    }
}