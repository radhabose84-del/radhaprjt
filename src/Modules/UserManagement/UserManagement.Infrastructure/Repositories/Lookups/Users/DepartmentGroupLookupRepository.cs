using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal class DepartmentGroupLookupRepository : IDepartmentGroupLookup
    {
        private readonly IDbConnection _dbConnection;

        public DepartmentGroupLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<DepartmentGroupLookupDto?> GetByIdAsync(int departmentGroupId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id AS DepartmentGroupId, DepartmentGroupName
                FROM AppData.DepartmentGroup
                WHERE Id = @DepartmentGroupId
                  AND IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<DepartmentGroupLookupDto>(
                new CommandDefinition(sql, new { DepartmentGroupId = departmentGroupId }, cancellationToken: ct));
        }
    }
}
