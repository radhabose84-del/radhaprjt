using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface IUserLookup
    {
        Task<UserLookupDto?> GetByIdAsync(int userId, CancellationToken ct = default);
        Task<IReadOnlyList<UserLookupDto>> GetByIdsAsync(IEnumerable<int> userIds, CancellationToken ct = default);
        Task<List<UserLookupDto>> GetAllUserAsync();
    }
}
