using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping
{
    public interface IRoleItemGroupMappingCommandRepository
    {
        Task<Domain.Entities.RoleItemGroupMapping> CreateAsync(Domain.Entities.RoleItemGroupMapping entity);
        Task<int> UpdateAsync(int id, Domain.Entities.RoleItemGroupMapping entity);
        Task<int> DeleteAsync(int id, Domain.Entities.RoleItemGroupMapping entity);
        Task<bool> CompositeKeyExistsAsync(int roleId, int itemGroupId, int? excludeId = null);
    }
}
