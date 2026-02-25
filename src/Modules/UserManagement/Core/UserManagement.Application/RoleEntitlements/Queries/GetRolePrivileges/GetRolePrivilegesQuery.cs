using UserManagement.Application.RoleEntitlements.Commands.GetRolePrivileges;
using MediatR;

namespace UserManagement.Application.RoleEntitlements.Queries.GetRolePrivileges
{
    public class GetRolePrivilegesQuery : IRequest<List<ModuleDTO>>
    {
        public int UserId { get; set; }
    }
}