using System.Data;
using Dapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using UserManagement.Application.Common.Interfaces;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal class UserLookupRepository : IUserLookup
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public UserLookupRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<List<UserLookupDto>> GetAllUserAsync()
        {
            const string sql = @"
                SELECT
                    UserId,
                    UserName,
                    FirstName,
                    LastName,
                    EmailId AS Email
                FROM [AppSecurity].[Users]
                WHERE IsDeleted = 0
                ORDER BY FirstName ASC;
            ";

            var result = await _dbConnection.QueryAsync<UserLookupDto>(sql);
            return result.ToList();
        }

        public async Task<UserLookupDto?> GetByIdAsync(int userId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT TOP 1
                    UserId,
                    UserName,
                    FirstName,
                    LastName,
                    EmailId AS Email
                FROM [AppSecurity].[Users]
                WHERE IsDeleted = 0 AND UserId = @UserId;
            ";

            return await _dbConnection.QueryFirstOrDefaultAsync<UserLookupDto>(
                new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));
        }

        public async Task<IReadOnlyList<UserLookupDto>> GetByIdsAsync(IEnumerable<int> userIds, CancellationToken ct = default)
        {
            var ids = userIds?.Distinct().ToArray() ?? Array.Empty<int>();
            if (ids.Length == 0)
                return Array.Empty<UserLookupDto>();

            const string sql = @"
                SELECT
                    UserId,
                    UserName,
                    FirstName,
                    LastName,
                    EmailId AS Email
                FROM [AppSecurity].[Users]
                WHERE IsDeleted = 0 AND UserId IN @Ids;
            ";

            var rows = await _dbConnection.QueryAsync<UserLookupDto>(
                new CommandDefinition(sql, new { Ids = ids }, cancellationToken: ct));

            return rows.ToList();
        }
    }
}
