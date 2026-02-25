namespace UserManagement.Application.Common.Interfaces.IUserRoleAllocation
{
    public interface IUserRoleAllocationCommandRepository
    {
    Task CreateAsync(UserManagement.Domain.Entities.UserRoleAllocation userRoleAllocation);
    Task AddRangeAsync(List<UserManagement.Domain.Entities.UserRoleAllocation> userRoleAllocations);
    Task UpdateAsync(UserManagement.Domain.Entities.UserRoleAllocation userRoleAllocation);
    Task DeleteAsync(int id);
    }
}