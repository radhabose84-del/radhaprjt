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
    internal class BinLookupRepository : IBinLookup
    {
        private readonly IDbConnection _dbConnection;

        public BinLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<BinLookupDto>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, WarehouseId, RackId, BinCode, BinName
                FROM Warehouse.BinMaster
                WHERE IsDeleted = 0
                ORDER BY BinCode, BinName;";

            var rows = await _dbConnection.QueryAsync<BinLookupDto>(
                new CommandDefinition(sql, cancellationToken: ct));

            return rows.AsList();
        }

        public async Task<IReadOnlyList<BinLookupDto>> GetByIdsAsync(IEnumerable<int> binIds, CancellationToken ct = default)
        {
            var ids = binIds?.ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<BinLookupDto>();

            const string sql = @"
                SELECT Id, WarehouseId, RackId, BinCode, BinName
                FROM Warehouse.BinMaster
                WHERE IsDeleted = 0
                  AND Id IN @BinIds
                ORDER BY BinCode, BinName;";

            var rows = await _dbConnection.QueryAsync<BinLookupDto>(
                new CommandDefinition(sql, new { BinIds = ids }, cancellationToken: ct));

            return rows.AsList();
        }

        public async Task<List<BinLookupDto>> GetByWarehouseIdAsync(int warehouseId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, WarehouseId, RackId, BinCode, BinName
                FROM Warehouse.BinMaster
                WHERE IsDeleted = 0
                  AND WarehouseId = @WarehouseId
                ORDER BY BinCode, BinName;";

            var rows = await _dbConnection.QueryAsync<BinLookupDto>(
                new CommandDefinition(sql, new { WarehouseId = warehouseId }, cancellationToken: ct));

            return rows.AsList();
        }

        public async Task<List<BinLookupDto>> GetByRackIdAsync(int rackId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, WarehouseId, RackId, BinCode, BinName
                FROM Warehouse.BinMaster
                WHERE IsDeleted = 0
                  AND RackId = @RackId
                ORDER BY BinCode, BinName;";

            var rows = await _dbConnection.QueryAsync<BinLookupDto>(
                new CommandDefinition(sql, new { RackId = rackId }, cancellationToken: ct));

            return rows.AsList();
        }
    }
}
