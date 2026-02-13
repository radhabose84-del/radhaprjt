using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;

namespace InventoryManagement.Infrastructure.Repositories.Lookups
{
    internal class HSNLookupRepository : IHSNLookup
    {
        private readonly IDbConnection _dbConnection;

        public HSNLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<HSNLookupDto>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT 
                    h.Id,
                    h.HSNCode,
                    h.[Description],
                    h.TypeId,
                    ht.[Code] AS TypeCode,
                    h.GSTPercentage,
                    h.CGSTPercentage,
                    h.SGSTPercentage,
                    h.IGSTPercentage,
                    h.IsActive
                FROM Inventory.HSNMaster h
                LEFT JOIN Inventory.MiscMaster ht ON h.TypeId = ht.Id
                WHERE h.IsDeleted = 0
                  AND h.IsActive = 1
                ORDER BY h.HSNCode;";

            var result = await _dbConnection.QueryAsync<HSNLookupDto>(
                new CommandDefinition(sql, cancellationToken: ct));

            return result.ToList();
        }

        public async Task<IReadOnlyList<HSNLookupDto>> GetByIdsAsync(IEnumerable<int> hsnIds, CancellationToken ct = default)
        {
            var ids = hsnIds?.ToList() ?? new List<int>();
            if (ids.Count == 0)
            {
                return Array.Empty<HSNLookupDto>();
            }

            const string sql = @"
                SELECT 
                    h.Id,
                    h.HSNCode,
                    h.[Description],
                    h.TypeId,
                    ht.[Code] AS TypeCode,
                    h.GSTPercentage,
                    h.CGSTPercentage,
                    h.SGSTPercentage,
                    h.IGSTPercentage,
                    h.IsActive
                FROM Inventory.HSNMaster h
                LEFT JOIN Inventory.MiscMaster ht ON h.TypeId = ht.Id
                WHERE h.Id IN @Ids
                  AND h.IsDeleted = 0
                  AND h.IsActive = 1;";

            var result = await _dbConnection.QueryAsync<HSNLookupDto>(
                new CommandDefinition(sql, new { Ids = ids }, cancellationToken: ct));

            return result.ToList();
        }
    }
}
