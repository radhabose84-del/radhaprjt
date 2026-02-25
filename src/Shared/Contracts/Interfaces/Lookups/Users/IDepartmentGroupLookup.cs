using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface IDepartmentGroupLookup
    {
        Task<DepartmentGroupLookupDto?> GetByIdAsync(int departmentGroupId, CancellationToken ct = default);
    }
}
