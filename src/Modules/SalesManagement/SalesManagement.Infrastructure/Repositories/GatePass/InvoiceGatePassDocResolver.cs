using System.Data;
using Contracts.Dtos.Common;
using Contracts.Interfaces.Gate;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;
using SalesManagement.Domain.Common;

namespace SalesManagement.Infrastructure.Repositories.GatePass
{
    internal sealed class InvoiceGatePassDocResolver : IGatePassDocResolver
    {
        public string DocumentType => MiscEnumEntity.TransactionTypeInvoice;

        private readonly IDbConnection _dbConnection;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;

        public InvoiceGatePassDocResolver(
            IDbConnection dbConnection,
            IItemLookup itemLookup,
            IUOMLookup uomLookup)
        {
            _dbConnection = dbConnection;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
        }

        public async Task<IReadOnlyList<GatePassDocSummaryDto>> GetSummariesAsync(IEnumerable<int> docIds, CancellationToken ct = default)
        {
            var ids = docIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<GatePassDocSummaryDto>();

            const string sql = @"
                SELECT
                    h.Id              AS InvoiceId,
                    h.TotalBags,
                    h.TotalWeight,
                    h.TransporterName,
                    first_det.ItemId  AS FirstItemId,
                    first_det.UOMId   AS FirstUOMId
                FROM Sales.InvoiceHeader h
                OUTER APPLY (
                    SELECT TOP 1 d.ItemId, d.UOMId
                    FROM Sales.InvoiceDetail d
                    WHERE d.InvoiceHeaderId = h.Id
                    ORDER BY d.ItemSno
                ) first_det
                WHERE h.Id IN @Ids AND h.IsDeleted = 0";

            var rows = (await _dbConnection.QueryAsync<InvoiceRow>(
                new CommandDefinition(sql, new { Ids = ids }, cancellationToken: ct))).ToList();

            if (rows.Count == 0)
                return new List<GatePassDocSummaryDto>();

            var itemIds = rows.Where(r => r.FirstItemId.HasValue).Select(r => r.FirstItemId!.Value).Distinct().ToList();
            var uomIds = rows.Where(r => r.FirstUOMId.HasValue).Select(r => r.FirstUOMId!.Value).Distinct().ToList();

            var itemDict = itemIds.Count > 0
                ? (await _itemLookup.GetByIdsAsync(itemIds, ct)).ToDictionary(i => i.Id, i => i.ItemName)
                : new Dictionary<int, string>();

            var uomDict = uomIds.Count > 0
                ? (await _uomLookup.GetByIdsAsync(uomIds, ct)).ToDictionary(u => u.Id, u => u.UOMName)
                : new Dictionary<int, string>();

            return rows.Select(r => new GatePassDocSummaryDto
            {
                DocId = r.InvoiceId,
                TotalQty = r.TotalBags,
                NetKgs = r.TotalWeight,
                GrossKgs = null,
                WithLoadKgs = null,
                WithoutLoadKgs = null,
                TotalWeight = null,
                TransporterName = r.TransporterName,
                ItemDescription = r.FirstItemId.HasValue && itemDict.TryGetValue(r.FirstItemId.Value, out var itemName) ? itemName : null,
                Uom = r.FirstUOMId.HasValue && uomDict.TryGetValue(r.FirstUOMId.Value, out var uomName) ? uomName : null
            }).ToList();
        }

        private sealed class InvoiceRow
        {
            public int InvoiceId { get; set; }
            public int TotalBags { get; set; }
            public decimal TotalWeight { get; set; }
            public string? TransporterName { get; set; }
            public int? FirstItemId { get; set; }
            public int? FirstUOMId { get; set; }
        }
    }
}
