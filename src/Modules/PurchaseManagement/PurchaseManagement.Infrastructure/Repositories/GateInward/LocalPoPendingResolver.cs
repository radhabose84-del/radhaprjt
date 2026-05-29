using System.Data;
using Contracts.Dtos.Common;
using Contracts.Interfaces.Gate;
using Dapper;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Infrastructure.Repositories.GateInward.Dto;

namespace PurchaseManagement.Infrastructure.Repositories.GateInward
{
    /// <summary>
    /// LPO resolver for the centralized Gate Inward Entry.
    /// Header listing + per-PO line items, status = Approved, qty remaining.
    /// </summary>
    internal sealed class LocalPoPendingResolver : IPendingReferenceDocResolver
    {
        public string DocumentTypeCode => "LPO";

        private readonly IDbConnection _dbConnection;

        public LocalPoPendingResolver(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<PendingReferenceDocDto>> GetPendingAsync(
            int partyId, int unitId, CancellationToken ct = default)
        {
            const string sql = @"
                WITH GrnSummary AS (
                    SELECT gd.PoId, SUM(gd.DcQuantity) AS TotalGrnQty
                    FROM Purchase.GrnDetail gd
                    INNER JOIN Purchase.GrnHeader gh ON gd.GrnId = gh.Id
                    GROUP BY gd.PoId
                ),
                LocalPO AS (
                    SELECT
                        poh.Id          AS DocId,
                        poh.PONumber    AS DocNumber,
                        poh.PODate      AS DocDate,
                        poh.VendorId    AS PartyId,
                        poh.UnitId      AS UnitId,
                        SUM(pld.Quantity) AS TotalOrderQty
                    FROM Purchase.PurchaseOrderHeader poh
                    INNER JOIN Purchase.PurchaseLocalHeader plh ON poh.Id  = plh.PurchaseOrderId
                    INNER JOIN Purchase.PurchaseLocalDetail pld ON plh.Id  = pld.PurchaseLocalId
                    INNER JOIN Purchase.MiscMaster mm           ON poh.StatusId = mm.Id
                    WHERE mm.Description = @ApprovedStatus
                      AND poh.IsDeleted  = 0
                      AND poh.VendorId   = @PartyId
                      AND poh.UnitId     = @UnitId
                    GROUP BY poh.Id, poh.PONumber, poh.PODate, poh.VendorId, poh.UnitId
                )
                SELECT lp.DocId, lp.DocNumber, lp.DocDate, lp.PartyId, lp.UnitId
                FROM   LocalPO lp
                LEFT JOIN GrnSummary gs ON gs.PoId = lp.DocId
                WHERE  (lp.TotalOrderQty - ISNULL(gs.TotalGrnQty, 0)) > 0
                ORDER BY lp.DocDate DESC, lp.DocId DESC;";

            var rows = await _dbConnection.QueryAsync<PendingReferenceDocDto>(
                new CommandDefinition(sql,
                    new
                    {
                        PartyId = partyId,
                        UnitId = unitId,
                        ApprovedStatus = MiscEnumEntity.Approved
                    },
                    cancellationToken: ct));

            return rows.AsList();
        }

        public async Task<IReadOnlyList<PendingReferenceDocLineDto>> GetPendingItemsAsync(
            IEnumerable<int> docIds, int partyId, int unitId, CancellationToken ct = default)
        {
            var ids = docIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<PendingReferenceDocLineDto>();

            const string sql = @"
                WITH GrnSummary AS (
                    SELECT PoId, ItemId, PoSlNoLocal, SUM(DcQuantity) AS TotalGrnQty
                    FROM   Purchase.GrnDetail
                    GROUP BY PoId, ItemId, PoSlNoLocal
                )
                SELECT
                    poh.Id                    AS DocId,
                    poh.PONumber              AS DocNumber,
                    poh.PODate                AS DocDate,
                    poh.VendorId              AS PartyId,
                    poh.UnitId                AS UnitId,
                    poh.POCategoryId          AS POCategoryId,
                    poh.POMethodId            AS POMethodId,
                    plh.IsPartialReceiptAllowed,
                    pld.ItemSno               AS PoSlNo,
                    pld.ItemId,
                    pld.Quantity              AS OrderQuantity,
                    ISNULL(gs.TotalGrnQty, 0)                            AS TotalGrnQty,
                    pld.Quantity - ISNULL(gs.TotalGrnQty, 0)             AS PendingQty
                FROM Purchase.PurchaseOrderHeader poh
                INNER JOIN Purchase.PurchaseLocalHeader plh ON poh.Id  = plh.PurchaseOrderId
                INNER JOIN Purchase.PurchaseLocalDetail  pld ON plh.Id = pld.PurchaseLocalId
                INNER JOIN Purchase.MiscMaster mm           ON poh.StatusId = mm.Id
                LEFT  JOIN GrnSummary gs
                       ON gs.PoId        = poh.Id
                      AND gs.ItemId      = pld.ItemId
                      AND gs.PoSlNoLocal = pld.ItemSno
                WHERE mm.Description = @ApprovedStatus
                  AND poh.IsDeleted  = 0
                  AND poh.VendorId   = @PartyId
                  AND poh.UnitId     = @UnitId
                  AND poh.Id IN @PoIds
                  AND (pld.Quantity - ISNULL(gs.TotalGrnQty, 0)) > 0
                ORDER BY poh.Id, pld.ItemSno;";

            var rows = (await _dbConnection.QueryAsync<PendingReferenceDocLineRow>(
                new CommandDefinition(sql,
                    new
                    {
                        PartyId = partyId,
                        UnitId = unitId,
                        PoIds = ids,
                        ApprovedStatus = MiscEnumEntity.Approved
                    },
                    cancellationToken: ct))).ToList();

            return rows
                .GroupBy(r => r.DocId)
                .Select(g =>
                {
                    var first = g.First();
                    return new PendingReferenceDocLineDto
                    {
                        DocId = first.DocId,
                        DocNumber = first.DocNumber,
                        DocDate = first.DocDate,
                        PartyId = first.PartyId,
                        UnitId = first.UnitId,
                        POCategoryId = first.POCategoryId,
                        POMethodId = first.POMethodId,
                        IsPartialReceiptAllowed = first.IsPartialReceiptAllowed,
                        Items = g.Select(r => new PendingReferenceDocLineItemDto
                        {
                            PoSlNo = r.PoSlNo,
                            ItemId = r.ItemId,
                            OrderQuantity = r.OrderQuantity,
                            TotalGrnQty = r.TotalGrnQty,
                            PendingQty = r.PendingQty
                        }).ToList()
                    };
                })
                .ToList();
        }
    }
}
