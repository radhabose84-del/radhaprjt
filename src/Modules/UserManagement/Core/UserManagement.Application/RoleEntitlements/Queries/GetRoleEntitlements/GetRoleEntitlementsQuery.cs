using MediatR;

namespace UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlements
{
    public class GetRoleEntitlementsQuery : IRequest<List<RoleEntitlementDto>>
    {
        public string RoleName { get; set; } = default!;
    }

}