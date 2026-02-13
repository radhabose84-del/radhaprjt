using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    public class CountryLookupRepository : ICountryLookup
    {
        private readonly IDbConnection _dbConnection;

        public CountryLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<CountryLookupDto>> GetAllCountriesAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    C.Id AS CountryId,
                    C.CountryName
                FROM [AppData].[Country] C
                WHERE C.IsDeleted = 0
                AND C.IsActive = 1
                ORDER BY C.Id DESC;
                ";

            var cmd = new CommandDefinition(sql, cancellationToken: ct);
            var rows = await _dbConnection.QueryAsync<CountryLookupDto>(cmd);

            return rows.ToList();
        }

        public async Task<CountryLookupDto?> GetByIdAsync(int countryId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    C.Id AS CountryId,
                    C.CountryName
                FROM [AppData].[Country] C
                WHERE C.Id = @CountryId
                AND C.IsDeleted = 0
                AND C.IsActive = 1;
                ";

            var cmd = new CommandDefinition(sql, new { CountryId = countryId }, cancellationToken: ct);
            return await _dbConnection.QueryFirstOrDefaultAsync<CountryLookupDto>(cmd);
        }

        public async Task<IReadOnlyList<CountryLookupDto>> GetByIdsAsync(IEnumerable<int> countryIds, CancellationToken ct = default)
        {
            var ids = (countryIds ?? Array.Empty<int>())
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (ids.Length == 0)
                return Array.Empty<CountryLookupDto>();

            const string sql = @"
                SELECT
                    C.Id AS CountryId,
                    C.CountryName
                FROM [AppData].[Country] C
                WHERE C.Id IN @Ids
                AND C.IsDeleted = 0
                AND C.IsActive = 1;
                ";

            var cmd = new CommandDefinition(sql, new { Ids = ids }, cancellationToken: ct);
            var rows = await _dbConnection.QueryAsync<CountryLookupDto>(cmd);

            return rows.ToList();
        }
    }
}
