using UserManagement.Application.AccessPolicy.Dto;

namespace UserManagement.Application.Common.Interfaces.IAccessPolicy
{
    public interface IAccessPolicyQueryRepository
    {
        Task<(List<AccessPolicyDto>, int)>        GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<AccessPolicyDto?>                     GetByIdAsync(int id);
        Task<IReadOnlyList<AccessPolicyDto>>       AutocompleteAsync(string term, CancellationToken ct);
        Task<bool>                                 AlreadyExistsAsync(string policyCode, int? excludeId = null);
        Task<bool>                                 NotFoundAsync(int id);
        Task<List<RoleAccessPolicyDto>>            GetRoleAccessPoliciesAsync(int accessPolicyId, int? roleId = null);
        Task<bool>                                 RoleValueAssignmentExistsAsync(int accessPolicyId, int roleId, int valueId, int? excludeId = null);
        Task<bool>                                 RoleAccessPolicyNotFoundAsync(int id);
        Task<bool>                                 UserRoleExistsAsync(int roleId);
    }
}
