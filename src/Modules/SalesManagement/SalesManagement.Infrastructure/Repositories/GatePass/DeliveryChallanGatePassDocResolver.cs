using System.Data;
using Contracts.Dtos.Common;
using Contracts.Interfaces.Gate;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using SalesManagement.Domain.Common;

namespace SalesManagement.Infrastructure.Repositories.GatePass
{
    internal sealed class DeliveryChallanGatePassDocResolver : IGatePassDocResolver
    {
        public string DocumentType => MiscEnumEntity.TransactionTypeStodc;

        private readonly IDbConnection _dbConnection;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IPartyLookup _partyLookup;

        public DeliveryChallanGatePassDocResolver(
            IDbConnection dbConnection,
            IItemLookup itemLookup,
            IUOMLookup uomLookup,
            IPartyLookup partyLookup)
        {
            _dbConnection = dbConnection;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
            _partyLookup = partyLookup;
        }

        public async Task<IReadOnlyList<GatePassDocSummaryDto>> GetSummariesAsync(IEnumerable<int> docIds, CancellationToken ct = default)
        {
            var ids = docIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<GatePassDocSummaryDto>();

            const string sql = @"
                SELECT
                    h.Id               AS DcId,
                    h.TransporterId,
                    first_det.ItemId   AS FirstItemId,
                    first_det.UOMId    AS FirstUOMId,
                    first_det.DispatchQuantity,
                    first_det.NetWeight,
                    first_det.GrossWeight
                FROM Sales.DeliveryChallanHeader h
                OUTER APPLY (
                    SELECT TOP 1 d.ItemId, d.UOMId, d.DispatchQuantity, d.NetWeight, d.GrossWeight
                    FROM Sales.DeliveryChallanDetail d
                    WHERE d.DeliveryChallanHeaderId = h.Id
                    ORDER BY d.Id
                ) first_det
                WHERE h.Id IN @Ids AND h.IsDeleted = 0";

            var rows = (await _dbConnection.QueryAsync<DcRow>(
                new CommandDefinition(sql, new { Ids = ids }, cancellationToken: ct))).ToList();

            if (rows.Count == 0)
                return new List<GatePassDocSummaryDto>();

            var itemIds = rows.Where(r => r.FirstItemId.HasValue).Select(r => r.FirstItemId!.Value).Distinct().ToList();
            var uomIds = rows.Where(r => r.FirstUOMId.HasValue).Select(r => r.FirstUOMId!.Value).Distinct().ToList();
            var partyIds = rows.Where(r => r.TransporterId.HasValue && r.TransporterId.Value > 0)
                               .Select(r => r.TransporterId!.Value).Distinct().ToList();

            var itemDict = itemIds.Count > 0
                ? (await _itemLookup.GetByIdsAsync(itemIds, ct)).ToDictionary(i => i.Id, i => i.ItemName)
                : new Dictionary<int, string>();

            var uomDict = uomIds.Count > 0
                ? (await _uomLookup.GetByIdsAsync(uomIds, ct)).ToDictionary(u => u.Id, u => u.UOMName)
                : new Dictionary<int, string>();

            var partyDict = partyIds.Count > 0
                ? (await _partyLookup.GetByIdsAsync(partyIds, ct)).ToDictionary(p => p.Id, p => p.PartyName)
                : new Dictionary<int, string>();

            return rows.Select(r => new GatePassDocSummaryDto
            {
                DocId = r.DcId,
                TotalQty = r.DispatchQuantity,
                NetKgs = r.NetWeight,
                GrossKgs = r.GrossWeight,
                WithLoadKgs = null,
                WithoutLoadKgs = null,
                TotalWeight = null,
                TransporterName = r.TransporterId.HasValue && partyDict.TryGetValue(r.TransporterId.Value, out var pName) ? pName : null,
                ItemDescription = r.FirstItemId.HasValue && itemDict.TryGetValue(r.FirstItemId.Value, out var itemName) ? itemName : null,
                Uom = r.FirstUOMId.HasValue && uomDict.TryGetValue(r.FirstUOMId.Value, out var uomName) ? uomName : null
            }).ToList();
        }

        private sealed class DcRow
        {
            public int DcId { get; set; }
            public int? TransporterId { get; set; }
            public int? FirstItemId { get; set; }
            public int? FirstUOMId { get; set; }
            public decimal? DispatchQuantity { get; set; }
            public decimal? NetWeight { get; set; }
            public decimal? GrossWeight { get; set; }
        }
    }
}
