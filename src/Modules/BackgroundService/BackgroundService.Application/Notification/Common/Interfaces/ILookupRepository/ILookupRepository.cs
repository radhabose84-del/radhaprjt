using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Users;

namespace BackgroundService.Application.Notification.Common.Interfaces
{
    public interface
    ILookupRepository
    {
        Task<IReadOnlyDictionary<int, string>> GetDepartmentsAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyDictionary<int, string>> GetUnitsAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyDictionary<int, string>> GetMenuNamesAsync(
            IEnumerable<int> menuIds,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyDictionary<int, string>> GetUserNamesAsync(
            IEnumerable<int> userIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the menu ID by menu name (workflow type name).
        /// </summary>
        Task<int?> GetMenuIdByNameAsync(
            string menuName,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyDictionary<int, string>> GetTransactionTypeNamesAsync(
            IEnumerable<int> transactionTypeIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the menu ID from Finance.TransactionTypeMaster by TransactionTypeId.
        /// Fallback when MenuName does not match any menu (e.g. "Contract Purchase Order"
        /// maps to MenuId 174 via TransactionTypeMaster).
        /// </summary>
        Task<int?> GetMenuIdByTransactionTypeIdAsync(
            int transactionTypeId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets user session by JWT ID.
        /// </summary>
        Task<UserSessionDto?> GetSessionByJwtIdAsync(
            string jwtId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates session last activity timestamp.
        /// </summary>
        Task<bool> UpdateSessionLastActivityAsync(
            string jwtId,
            DateTimeOffset lastActivity,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the set of MenuIds accessible to a user based on their role-based menu privileges (CanView).
        /// </summary>
        Task<HashSet<int>> GetUserAccessibleMenuIdsAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
