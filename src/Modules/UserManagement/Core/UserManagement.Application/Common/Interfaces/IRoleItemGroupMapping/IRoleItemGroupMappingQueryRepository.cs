using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping
{
    public interface IRoleItemGroupMappingQueryRepository
    {
        Task<Domain.Entities.RoleItemGroupMapping?> GetByIdAsync(int id);
        Task<(List<Domain.Entities.RoleItemGroupMapping>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<List<Domain.Entities.RoleItemGroupMapping>> GetByRoleIdAsync(int roleId);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SoftDeleteValidation(int id);
    }
}
