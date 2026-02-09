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
    internal class CurrencyLookupRepository : ICurrencyLookup
    {
        private readonly IDbConnection _dbConnection;

        public CurrencyLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<CurrencyLookupDto>> GetByIdsAsync(IEnumerable<int> currencyIds, CancellationToken ct = default)
        {
            var ids = currencyIds?
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            if (ids == null || ids.Length == 0)
                return Array.Empty<CurrencyLookupDto>();

            const string sql = @"
                SELECT
                    Id       AS CurrencyId,
                    Code,
                    Name
                FROM [AppData].[Currency]
                WHERE IsDeleted = 0
                  AND Id IN @CurrencyIds;
            ";

            var result = await _dbConnection.QueryAsync<CurrencyLookupDto>(
                new CommandDefinition(
                    sql,
                    new { CurrencyIds = ids },
                    cancellationToken: ct));

            return result.ToList();
        }
    }
}
