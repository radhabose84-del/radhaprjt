using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;

namespace WarehouseManagement.Infrastructure.Repositories.Lookups
{
    internal class RackLookupRepository : IRackLookup
    {
        private readonly IDbConnection _dbConnection;

        public RackLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<RackLookupDto>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, WarehouseId, RackCode, RackName
                FROM Warehouse.RackMaster
                WHERE IsDeleted = 0
                ORDER BY RackCode, RackName;";

            var rows = await _dbConnection.QueryAsync<RackLookupDto>(
                new CommandDefinition(sql, cancellationToken: ct));

            return rows.AsList();
        }

        public async Task<IReadOnlyList<RackLookupDto>> GetByIdsAsync(IEnumerable<int> rackIds, CancellationToken ct = default)
        {
            var ids = rackIds?.ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<RackLookupDto>();

            const string sql = @"
                SELECT Id, WarehouseId, RackCode, RackName
                FROM Warehouse.RackMaster
                WHERE IsDeleted = 0
                  AND Id IN @RackIds
                ORDER BY RackCode, RackName;";

            var rows = await _dbConnection.QueryAsync<RackLookupDto>(
                new CommandDefinition(sql, new { RackIds = ids }, cancellationToken: ct));

            return rows.AsList();
        }

        public async Task<List<RackLookupDto>> GetByWarehouseIdAsync(int warehouseId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, WarehouseId, RackCode, RackName
                FROM Warehouse.RackMaster
                WHERE IsDeleted = 0
                  AND WarehouseId = @WarehouseId
                ORDER BY RackCode, RackName;";

            var rows = await _dbConnection.QueryAsync<RackLookupDto>(
                new CommandDefinition(sql, new { WarehouseId = warehouseId }, cancellationToken: ct));

            return rows.AsList();
        }
    }
}
