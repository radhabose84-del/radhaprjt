using System.Data;
using Contracts.Dtos.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.QC;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Lookups.Purchase
{
    internal sealed class GrnLookupRepository : IGrnLookup
    {
        private readonly IDbConnection _dbConnection;
        private readonly IItemLookup _itemLookup;
        private readonly IQualitySpecificationLookup _qualitySpecificationLookup;

        public GrnLookupRepository(
            IDbConnection dbConnection,
            IItemLookup itemLookup,
            IQualitySpecificationLookup qualitySpecificationLookup)
        {
            _dbConnection = dbConnection;
            _itemLookup = itemLookup;
            _qualitySpecificationLookup = qualitySpecificationLookup;
        }

        public async Task<GrnLookupDto?> GetByGrnDetailIdAsync(int grnDetailId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT  gd.GrnId            AS GrnHeaderId,
                        gd.Id               AS GrnDetailId,
                        gh.GrnNo            AS GrnNo,
                        gh.GrnDate          AS GrnDate,
                        gh.PartyId          AS SupplierId,
                        gh.InvoiceNo        AS InvoiceNo,
                        gd.ItemId           AS ItemId,
                        gd.BatchNumber      AS BatchNumber,
                        gd.ReceivedQuantity AS ReceivedQuantity,
                        gd.UOMId            AS ReceivedUomId
                FROM Purchase.GrnDetail gd
                INNER JOIN Purchase.GrnHeader gh ON gh.Id = gd.GrnId
                WHERE gd.Id = @GrnDetailId;";

            return await _dbConnection.QueryFirstOrDefaultAsync<GrnLookupDto>(
                new CommandDefinition(sql, new { GrnDetailId = grnDetailId }, cancellationToken: ct));
        }

        public async Task<IReadOnlyList<GrnLookupDto>> GetByGrnDetailIdsAsync(
            IEnumerable<int> grnDetailIds, CancellationToken ct = default)
        {
            var ids = grnDetailIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<GrnLookupDto>();

            const string sql = @"
                SELECT  gd.GrnId            AS GrnHeaderId,
                        gd.Id               AS GrnDetailId,
                        gh.GrnNo            AS GrnNo,
                        gh.GrnDate          AS GrnDate,
                        gh.PartyId          AS SupplierId,
                        gh.InvoiceNo        AS InvoiceNo,
                        gd.ItemId           AS ItemId,
                        gd.BatchNumber      AS BatchNumber,
                        gd.ReceivedQuantity AS ReceivedQuantity,
                        gd.UOMId            AS ReceivedUomId
                FROM Purchase.GrnDetail gd
                INNER JOIN Purchase.GrnHeader gh ON gh.Id = gd.GrnId
                WHERE gd.Id IN @Ids;";

            var result = await _dbConnection.QueryAsync<GrnLookupDto>(
                new CommandDefinition(sql, new { Ids = ids }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<IReadOnlyList<GrnLookupDto>> GetGrnLinesAsync(
            int? supplierId, DateTimeOffset? fromDate, DateTimeOffset? toDate, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT  gd.GrnId            AS GrnHeaderId,
                        gd.Id               AS GrnDetailId,
                        gh.GrnNo            AS GrnNo,
                        gh.GrnDate          AS GrnDate,
                        gh.PartyId          AS SupplierId,
                        gh.InvoiceNo        AS InvoiceNo,
                        gd.ItemId           AS ItemId,
                        gd.BatchNumber      AS BatchNumber,
                        gd.ReceivedQuantity AS ReceivedQuantity,
                        gd.UOMId            AS ReceivedUomId
                FROM Purchase.GrnDetail gd
                INNER JOIN Purchase.GrnHeader gh ON gh.Id = gd.GrnId
                WHERE gh.IsGrnGenerated = 1
                  AND (@SupplierId IS NULL OR gh.PartyId = @SupplierId)
                  AND (@FromDate   IS NULL OR gh.GrnDate >= @FromDate)
                  AND (@ToDate     IS NULL OR gh.GrnDate <= @ToDate)
                ORDER BY gh.GrnDate DESC, gd.Id ASC;";

            var lines = (await _dbConnection.QueryAsync<GrnLookupDto>(
                new CommandDefinition(
                    sql,
                    new { SupplierId = supplierId, FromDate = fromDate, ToDate = toDate },
                    cancellationToken: ct))).ToList();

            await PopulateTemplateAvailabilityAsync(lines, ct);
            return lines;
        }

        // QC template availability per line — same Y/N rule as ArrivalQueryRepository.LoadDetailsAsync:
        // the item (or its item category) has an active Qc.QualitySpecification.
        private async Task PopulateTemplateAvailabilityAsync(List<GrnLookupDto> lines, CancellationToken ct)
        {
            if (lines.Count == 0)
                return;

            var distinctItemIds = lines.Select(l => l.ItemId).Where(i => i > 0).Distinct().ToList();
            if (distinctItemIds.Count == 0)
            {
                foreach (var l in lines)
                    l.IsTemplateAvailable = "N";
                return;
            }

            var itemCategoryMap = (await _itemLookup.GetByIdsAsync(distinctItemIds, ct))
                .ToDictionary(x => x.Id, x => x.ItemCategoryId);
            var categoryIds = itemCategoryMap.Values.Where(c => c > 0).Distinct().ToList();

            var templateMatch = await _qualitySpecificationLookup.GetMatchingAsync(distinctItemIds, categoryIds, ct);

            foreach (var l in lines)
            {
                var itemMatched = templateMatch.MatchedItemIds.Contains(l.ItemId);
                var categoryMatched = itemCategoryMap.TryGetValue(l.ItemId, out var catId)
                    && catId > 0 && templateMatch.MatchedItemCategoryIds.Contains(catId);
                l.IsTemplateAvailable = (itemMatched || categoryMatched) ? "Y" : "N";
            }
        }

        public async Task<int> GetLineCountAsync(int grnHeaderId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM Purchase.GrnDetail
                WHERE GrnId = @GrnHeaderId;";

            return await _dbConnection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { GrnHeaderId = grnHeaderId }, cancellationToken: ct));
        }
    }
}
