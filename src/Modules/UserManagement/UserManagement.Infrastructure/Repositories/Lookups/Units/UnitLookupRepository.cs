using System.Data;
using Dapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;

namespace UserManagement.Infrastructure.Repositories.Lookups.Units
{
    /// <summary>
    /// Read-only lookup repository for Unit.
    /// Owning module: UserManagement
    /// Used by other modules (Maintenance, FAM, etc.) via lookup interfaces.
    /// </summary>
    internal class UnitLookupRepository : IUnitLookup
    {
        private readonly IDbConnection _dbConnection;

        public UnitLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        /// <summary>
        /// Get single unit lookup by Id
        /// </summary>
        public async Task<UnitLookupDto?> GetByIdAsync(
            int unitId,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT TOP 1
                    U.Id       AS UnitId,
                    U.UnitName AS UnitName
                FROM [AppData].[Unit] U
                WHERE U.IsDeleted = 0
                  AND U.Id = @UnitId;
            ";

            return await _dbConnection.QueryFirstOrDefaultAsync<UnitLookupDto>(
                new CommandDefinition(
                    sql,
                    new { UnitId = unitId },
                    cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Bulk lookup for units by Ids (used in listing screens)
        /// </summary>
        public async Task<IReadOnlyList<UnitLookupDto>> GetByIdsAsync(
            IEnumerable<int> unitIds,
            CancellationToken cancellationToken = default)
        {
            var ids = unitIds?
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            if (ids == null || ids.Length == 0)
                return Array.Empty<UnitLookupDto>();

            const string sql = @"
                SELECT
                    U.Id       AS UnitId,
                    U.UnitName AS UnitName
                FROM [AppData].[Unit] U
                WHERE U.IsDeleted = 0
                  AND U.Id IN @UnitIds;
            ";

            var result = await _dbConnection.QueryAsync<UnitLookupDto>(
                new CommandDefinition(
                    sql,
                    new { UnitIds = ids },
                    cancellationToken: cancellationToken));

            return result.ToList();
        }
    }
}
