namespace UserManagement.Application.Common.Interfaces.IAccessPolicy
{
    public interface IAccessPolicyCommandRepository
    {
        Task<int>  CreateAsync(Domain.Entities.AccessPolicy entity);
        Task<int>  UpdateAsync(Domain.Entities.AccessPolicy entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
        Task<int>  AssignRoleValueAsync(Domain.Entities.RoleAccessPolicy entity);
        Task<bool> RemoveRoleValueAsync(int id, CancellationToken ct);
    }
}
