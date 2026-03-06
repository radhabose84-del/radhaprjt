using System.Data;
using Dapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
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
                    U.UnitName AS UnitName,
                    U.ShortName,
                    U.UnitHeadName,
                    U.OldUnitId,
                    U.SpindlesCapacity,
                    U.UnitTypeId,
                    MM.Description AS UnitTypeName
                FROM [AppData].[Unit] U
                LEFT JOIN [AppData].[MiscMaster] MM ON MM.Id = U.UnitTypeId AND MM.IsDeleted = 0
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
                    U.UnitName AS UnitName,
                    U.ShortName,
                    U.UnitHeadName,
                    U.OldUnitId,
                    U.SpindlesCapacity,
                    U.UnitTypeId,
                    MM.Description AS UnitTypeName
                FROM [AppData].[Unit] U
                LEFT JOIN [AppData].[MiscMaster] MM ON MM.Id = U.UnitTypeId AND MM.IsDeleted = 0
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

        /// <summary>
        /// Get all active units
        /// </summary>
        public async Task<List<UnitLookupDto>> GetAllUnitAsync()
        {
            const string sql = @"
                SELECT
                    U.Id        AS UnitId,
                    U.UnitName  AS UnitName,
                    U.ShortName AS ShortName,
                    U.OldUnitId AS OldUnitId,
                    U.SpindlesCapacity,
                    U.UnitTypeId,
                    MM.Description AS UnitTypeName
                FROM [AppData].[Unit] U
                LEFT JOIN [AppData].[MiscMaster] MM ON MM.Id = U.UnitTypeId AND MM.IsDeleted = 0
                WHERE U.IsDeleted = 0
                ORDER BY U.UnitName ASC;
            ";

            var result = await _dbConnection.QueryAsync<UnitLookupDto>(sql);
            return result.ToList();
        }

        /// <summary>
        /// Get units assigned to a specific user by userId
        /// </summary>
        public async Task<List<UnitLookupDto>> GetUserUnitAsync(int userId)
        {
            const string sql = @"
                SELECT
                    C.Id        AS UnitId,
                    C.UnitName  AS UnitName,
                    C.ShortName AS ShortName,
                    C.OldUnitId AS OldUnitId,
                    C.SpindlesCapacity,
                    C.UnitTypeId,
                    MM.Description AS UnitTypeName
                FROM [AppSecurity].[UserUnit] B
                INNER JOIN [AppData].[Unit] C ON B.UnitId = C.Id
                LEFT JOIN [AppData].[MiscMaster] MM ON MM.Id = C.UnitTypeId AND MM.IsDeleted = 0
                WHERE B.IsActive = 1
                  AND C.IsDeleted = 0
                  AND B.UserId = @UserId
                ORDER BY C.UnitName ASC;
            ";

            var result = await _dbConnection.QueryAsync<UnitLookupDto>(sql, new { UserId = userId });
            return result.ToList();
        }

        /// <summary>
        /// Get units assigned to a specific user by userName
        /// </summary>
        public async Task<List<UnitLookupDto>> GetUserUnitByUserNameAsync(string userName)
        {
            const string sql = @"
                SELECT
                    C.Id        AS UnitId,
                    C.UnitName  AS UnitName,
                    C.ShortName AS ShortName,
                    C.OldUnitId AS OldUnitId,
                    C.SpindlesCapacity,
                    C.UnitTypeId,
                    MM.Description AS UnitTypeName
                FROM [AppSecurity].[Users] A
                INNER JOIN [AppSecurity].[UserUnit] B ON A.UserId = B.UserId
                INNER JOIN [AppData].[Unit] C ON B.UnitId = C.Id
                LEFT JOIN [AppData].[MiscMaster] MM ON MM.Id = C.UnitTypeId AND MM.IsDeleted = 0
                WHERE B.IsActive = 1
                  AND A.IsActive = 1
                  AND C.IsDeleted = 0
                  AND A.UserName = @UserName
                ORDER BY C.UnitName ASC;
            ";

            var result = await _dbConnection.QueryAsync<UnitLookupDto>(sql, new { UserName = userName });
            return result.ToList();
        }
    }
}
