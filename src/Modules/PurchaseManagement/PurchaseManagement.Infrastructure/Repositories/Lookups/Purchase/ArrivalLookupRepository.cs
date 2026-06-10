using System.Data;
using Contracts.Dtos.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Purchase;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Lookups.Purchase
{
    internal sealed class ArrivalLookupRepository : IArrivalLookup
    {
        private readonly IDbConnection _dbConnection;

        public ArrivalLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // ArrivalDetail has no IsDeleted column; the header carries soft-delete.
        private const string LineSelect = @"
            SELECT  d.ArrivalHeaderId AS ArrivalHeaderId,
                    d.Id              AS ArrivalDetailId,
                    h.ArrivalNumber   AS ArrivalNumber,
                    h.ArrivalDate     AS ArrivalDate,
                    h.SupplierId      AS SupplierId,
                    d.ItemId          AS ItemId,
                    d.BatchNumber     AS BatchNumber,
                    d.ArrivedQty      AS ReceivedQuantity,
                    d.UomId           AS ReceivedUomId
            FROM Purchase.ArrivalDetail d
            INNER JOIN Purchase.ArrivalHeader h ON h.Id = d.ArrivalHeaderId AND h.IsDeleted = 0";

        public async Task<ArrivalLookupDto?> GetByArrivalDetailIdAsync(int arrivalDetailId, CancellationToken ct = default)
        {
            var sql = $"{LineSelect} WHERE d.Id = @ArrivalDetailId;";
            return await _dbConnection.QueryFirstOrDefaultAsync<ArrivalLookupDto>(
                new CommandDefinition(sql, new { ArrivalDetailId = arrivalDetailId }, cancellationToken: ct));
        }

        public async Task<IReadOnlyList<ArrivalLookupDto>> GetByArrivalDetailIdsAsync(
            IEnumerable<int> arrivalDetailIds, CancellationToken ct = default)
        {
            var ids = arrivalDetailIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<ArrivalLookupDto>();

            var sql = $"{LineSelect} WHERE d.Id IN @Ids;";
            var result = await _dbConnection.QueryAsync<ArrivalLookupDto>(
                new CommandDefinition(sql, new { Ids = ids }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<IReadOnlyList<ArrivalLookupDto>> GetArrivalLinesAsync(
            int? supplierId, DateTimeOffset? fromDate, DateTimeOffset? toDate, CancellationToken ct = default)
        {
            var sql = $@"{LineSelect}
                  AND (@SupplierId IS NULL OR h.SupplierId = @SupplierId)
                  AND (@FromDate   IS NULL OR h.ArrivalDate >= @FromDate)
                  AND (@ToDate     IS NULL OR h.ArrivalDate <= @ToDate)
                ORDER BY h.ArrivalDate DESC, d.Id ASC;";

            var result = await _dbConnection.QueryAsync<ArrivalLookupDto>(
                new CommandDefinition(
                    sql,
                    new { SupplierId = supplierId, FromDate = fromDate, ToDate = toDate },
                    cancellationToken: ct));
            return result.ToList();
        }

        public async Task<int> GetLineCountAsync(int arrivalHeaderId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM Purchase.ArrivalDetail
                WHERE ArrivalHeaderId = @ArrivalHeaderId;";

            return await _dbConnection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { ArrivalHeaderId = arrivalHeaderId }, cancellationToken: ct));
        }
    }
}
