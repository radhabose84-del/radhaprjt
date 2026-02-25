using System.Data;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;

namespace InventoryManagement.Infrastructure.Repositories.Lookups
{
    internal class UOMLookupRepository : IUOMLookup
    {
        private readonly IDbConnection _dbConnection;

        public UOMLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<UOMLookupDto>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, Code, UOMName
                FROM Inventory.UOM
                WHERE IsDeleted = 0 AND IsActive = 1
                ORDER BY SortOrder;";

            var result = await _dbConnection.QueryAsync<UOMLookupDto>(
                new CommandDefinition(sql, cancellationToken: ct));

            return result.ToList();
        }

        public async Task<IReadOnlyList<UOMLookupDto>> GetByIdsAsync(IEnumerable<int> uomIds, CancellationToken ct = default)
        {
            var ids = uomIds?.ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<UOMLookupDto>();

            const string sql = @"
                SELECT Id, Code, UOMName
                FROM Inventory.UOM
                WHERE Id IN @UomIds AND IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<UOMLookupDto>(
                new CommandDefinition(sql, new { UomIds = ids }, cancellationToken: ct));

            return result.ToList();
        }
    }
}
