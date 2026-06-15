using MediatR;
using UserManagement.Application.AccessPolicy.Dto;

namespace UserManagement.Application.AccessPolicy.Queries.GetRoleAccessPolicies
{
    public class GetRoleAccessPoliciesQuery : IRequest<List<RoleAccessPolicyDto>>
    {
        public int  AccessPolicyId { get; set; }
        public int? RoleId         { get; set; }
    }
}
