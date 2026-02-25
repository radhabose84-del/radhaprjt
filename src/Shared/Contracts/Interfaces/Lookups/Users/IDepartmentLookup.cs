using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface IDepartmentLookup
    {
        Task<DepartmentLookupDto?> GetByIdAsync(int departmentId, CancellationToken ct = default);
        Task<IReadOnlyList<DepartmentLookupDto>> GetByIdsAsync(IEnumerable<int> departmentIds, CancellationToken ct = default);
        Task<List<DepartmentLookupDto>> GetAllDepartmentAsync();        
    }
}