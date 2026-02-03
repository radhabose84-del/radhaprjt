using System.Data;
using Dapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal class DepartmentLookupRepository : IDepartmentLookup
    {
        private readonly IDbConnection _dbConnection;

        public DepartmentLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<DepartmentLookupDto?> GetByIdAsync(int departmentId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT TOP 1
                    Id       AS DepartmentId,
                    DeptName AS DeptName
                FROM [AppData].[Department]
                WHERE IsDeleted = 0 AND Id = @Id;
            ";

            return await _dbConnection.QueryFirstOrDefaultAsync<DepartmentLookupDto>(
                new CommandDefinition(sql, new { Id = departmentId }, cancellationToken: ct));
        }

        public async Task<IReadOnlyList<DepartmentLookupDto>> GetByIdsAsync(IEnumerable<int> departmentIds, CancellationToken ct = default)
        {
            var ids = departmentIds?.Distinct().ToArray() ?? Array.Empty<int>();
            if (ids.Length == 0)
                return Array.Empty<DepartmentLookupDto>();

            const string sql = @"
                SELECT
                    Id       AS DepartmentId,
                    DeptName AS DeptName
                FROM [AppData].[Department]
                WHERE IsDeleted = 0 AND Id IN @Ids;
            ";

            var rows = await _dbConnection.QueryAsync<DepartmentLookupDto>(
                new CommandDefinition(sql, new { Ids = ids }, cancellationToken: ct));

            return rows.ToList();
        }
    }
}
