using MediatR;

namespace UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlements
{
    public class GetRoleEntitlementsQueryHandler : IRequestHandler<GetRoleEntitlementsQuery, List<RoleEntitlementDto>>
    {
        public Task<List<RoleEntitlementDto>> Handle(GetRoleEntitlementsQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<RoleEntitlementDto>());
        }
    }
}
