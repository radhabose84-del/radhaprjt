using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseReturn;

public sealed class PurchaseReturnQueryRepository : IPurchaseReturnQueryRepository
{
    private readonly IDbConnection _db;
    private readonly IPartyLookup _partyLookup;
    private readonly IItemLookup _itemLookup;
    private readonly IUOMLookup _uomLookup;

    public PurchaseReturnQueryRepository(
        IDbConnection db,
        IPartyLookup partyLookup,
        IItemLookup itemLookup,
        IUOMLookup uomLookup)
    {
        _db = db;
        _partyLookup = partyLookup;
        _itemLookup = itemLookup;
        _uomLookup = uomLookup;
    }

    public async Task<PurchaseReturnHeaderDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        const string headerSql = @"
            SELECT
                h.Id, h.RtvNumber, h.RtvDate, h.UnitId, h.VendorId,
                h.PoId, po.PONumber AS PoNumber,
                h.GrnHeaderId, gh.GrnNo,
                h.ReturnTypeId,   rt.Code AS ReturnTypeCode,   rt.Description AS ReturnTypeDescription,
                h.ReturnReasonId, rr.Code AS ReturnReasonCode, rr.Description AS ReturnReasonDescription,
                h.ReturnActionId, mAction.Code AS ReturnActionCode,
                h.IsReplacementRequired, h.IsDebitNoteRequired, h.IsQcVerified,
                h.Remarks,
                h.StatusId, mStatus.Code AS StatusCode,
                h.ApprovalRequestId,
                h.ReplacementStatusId, mRepl.Code AS ReplacementStatusCode,
                h.ReplacementClosedDate,
                h.IsActive, h.IsDeleted
            FROM Purchase.PurchaseReturnHeader h WITH (NOLOCK)
            LEFT JOIN Purchase.PurchaseOrderHeader po WITH (NOLOCK) ON po.Id = h.PoId
            LEFT JOIN Purchase.GrnHeader gh WITH (NOLOCK) ON gh.Id = h.GrnHeaderId
            LEFT JOIN Purchase.ReturnType rt WITH (NOLOCK) ON rt.Id = h.ReturnTypeId
            LEFT JOIN Purchase.ReturnReason rr WITH (NOLOCK) ON rr.Id = h.ReturnReasonId
            LEFT JOIN Purchase.MiscMaster mAction WITH (NOLOCK) ON mAction.Id = h.ReturnActionId
            LEFT JOIN Purchase.MiscMaster mStatus WITH (NOLOCK) ON mStatus.Id = h.StatusId
            LEFT JOIN Purchase.MiscMaster mRepl   WITH (NOLOCK) ON mRepl.Id = h.ReplacementStatusId
            WHERE h.Id = @id AND h.IsDeleted = 0;";

        var header = await _db.QueryFirstOrDefaultAsync<PurchaseReturnHeaderDto>(
            new CommandDefinition(headerSql, new { id }, cancellationToken: ct));
        if (header == null) return null;

        const string detailSql = @"
            SELECT
                d.Id, d.PurchaseReturnHeaderId, d.GrnDetailId,
                d.ItemId, d.UomId,
                d.ReceivedQty, d.AcceptedQty, d.ReturnQty,
                d.RatePerUnit, d.LineValue,
                d.ReturnReasonId, rr.Code AS ReturnReasonName,
                d.LineRemarks,
                d.IsActive
            FROM Purchase.PurchaseReturnDetail d WITH (NOLOCK)
            LEFT JOIN Purchase.ReturnReason rr WITH (NOLOCK) ON rr.Id = d.ReturnReasonId
            WHERE d.PurchaseReturnHeaderId = @id AND d.IsDeleted = 0;";

        var details = (await _db.QueryAsync<PurchaseReturnDetailDto>(
            new CommandDefinition(detailSql, new { id }, cancellationToken: ct))).AsList();

        // Cross-module enrichment
        var vendor = await _partyLookup.GetByIdAsync(header.VendorId, ct);
        header.VendorName = vendor?.PartyName;

        if (details.Count > 0)
        {
            var itemIds = details.Select(d => d.ItemId).Distinct().ToList();
            var uomIds = details.Select(d => d.UomId).Where(x => x > 0).Distinct().ToList();

            var items = await _itemLookup.GetByIdsAsync(itemIds, ct);
            var itemMap = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

            var uoms = uomIds.Count > 0
                ? await _uomLookup.GetByIdsAsync(uomIds, ct)
                : new List<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>();
            var uomMap = uoms.ToDictionary(u => u.Id, u => u.UOMName);

            foreach (var d in details)
            {
                if (itemMap.TryGetValue(d.ItemId, out var im))
                {
                    d.ItemCode = im.ItemCode;
                    d.ItemName = im.ItemName;
                }
                if (uomMap.TryGetValue(d.UomId, out var uname))
                    d.UomName = uname;
            }
        }

        header.Details = details;
        return header;
    }

    public async Task<(IReadOnlyList<PurchaseReturnListItemDto> Items, int Total)> GetAllAsync(
        int page, int size, string? search, CancellationToken ct)
    {
        page = page <= 0 ? 1 : page;
        size = size <= 0 ? 20 : size;
        var off = (page - 1) * size;

        const string sql = @"
            DECLARE @s NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@search)), '');

            SELECT COUNT(1)
            FROM Purchase.PurchaseReturnHeader h WITH (NOLOCK)
            WHERE h.IsDeleted = 0
              AND (@s IS NULL OR h.RtvNumber LIKE '%' + @s + '%');

            SELECT
                h.Id, h.RtvNumber, h.RtvDate,
                h.VendorId,
                h.PoId, po.PONumber AS PoNumber,
                h.GrnHeaderId, gh.GrnNo,
                h.ReturnTypeId,   rt.Code AS ReturnTypeCode,
                h.ReturnReasonId, rr.Code AS ReturnReasonCode,
                h.StatusId, mStatus.Code AS StatusCode,
                h.IsReplacementRequired, h.IsDebitNoteRequired,
                h.IsActive
            FROM Purchase.PurchaseReturnHeader h WITH (NOLOCK)
            LEFT JOIN Purchase.PurchaseOrderHeader po WITH (NOLOCK) ON po.Id = h.PoId
            LEFT JOIN Purchase.GrnHeader gh WITH (NOLOCK) ON gh.Id = h.GrnHeaderId
            LEFT JOIN Purchase.ReturnType rt WITH (NOLOCK) ON rt.Id = h.ReturnTypeId
            LEFT JOIN Purchase.ReturnReason rr WITH (NOLOCK) ON rr.Id = h.ReturnReasonId
            LEFT JOIN Purchase.MiscMaster mStatus WITH (NOLOCK) ON mStatus.Id = h.StatusId
            WHERE h.IsDeleted = 0
              AND (@s IS NULL OR h.RtvNumber LIKE '%' + @s + '%')
            ORDER BY h.Id DESC
            OFFSET @off ROWS FETCH NEXT @size ROWS ONLY;";

        using var multi = await _db.QueryMultipleAsync(
            new CommandDefinition(sql, new { search, off, size }, cancellationToken: ct));
        var total = await multi.ReadFirstAsync<int>();
        var items = (await multi.ReadAsync<PurchaseReturnListItemDto>()).AsList();

        if (items.Count > 0)
        {
            var vendorIds = items.Select(i => i.VendorId).Where(x => x > 0).Distinct().ToList();
            var vendors = await _partyLookup.GetByIdsAsync(vendorIds, ct);
            var vendorMap = vendors.ToDictionary(v => v.Id, v => v.PartyName ?? string.Empty);

            foreach (var i in items)
            {
                if (vendorMap.TryGetValue(i.VendorId, out var name))
                    i.VendorName = name;
            }
        }

        return (items, total);
    }

    public async Task<IReadOnlyList<PurchaseReturnListItemDto>> AutocompleteAsync(string? term, CancellationToken ct)
    {
        const string sql = @"
            SELECT TOP 20
                h.Id, h.RtvNumber, h.RtvDate,
                h.VendorId, h.PoId, po.PONumber AS PoNumber,
                h.GrnHeaderId, gh.GrnNo,
                h.ReturnTypeId,   rt.Code AS ReturnTypeCode,
                h.ReturnReasonId, rr.Code AS ReturnReasonCode,
                h.StatusId, mStatus.Code AS StatusCode,
                h.IsReplacementRequired, h.IsDebitNoteRequired,
                h.IsActive
            FROM Purchase.PurchaseReturnHeader h WITH (NOLOCK)
            LEFT JOIN Purchase.PurchaseOrderHeader po WITH (NOLOCK) ON po.Id = h.PoId
            LEFT JOIN Purchase.GrnHeader gh WITH (NOLOCK) ON gh.Id = h.GrnHeaderId
            LEFT JOIN Purchase.ReturnType rt WITH (NOLOCK) ON rt.Id = h.ReturnTypeId
            LEFT JOIN Purchase.ReturnReason rr WITH (NOLOCK) ON rr.Id = h.ReturnReasonId
            LEFT JOIN Purchase.MiscMaster mStatus WITH (NOLOCK) ON mStatus.Id = h.StatusId
            WHERE h.IsDeleted = 0 AND h.IsActive = 1
              AND (@term IS NULL OR @term = '' OR h.RtvNumber LIKE '%' + @term + '%')
            ORDER BY h.Id DESC;";
        var rows = await _db.QueryAsync<PurchaseReturnListItemDto>(
            new CommandDefinition(sql, new { term }, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<ReturnableQtyDto>> GetReturnableQtyByGrnAsync(int grnHeaderId, CancellationToken ct)
    {
        const string sql = @"
            SELECT
                gd.Id AS GrnDetailId,
                gd.ItemId,
                gd.UOMId AS UomId,
                gd.ReceivedQuantity AS ReceivedQty,
                ISNULL(gd.QcAcceptedQuantity, 0) AS AcceptedQty,
                ISNULL((
                    SELECT SUM(prd.ReturnQty)
                    FROM Purchase.PurchaseReturnDetail prd WITH (NOLOCK)
                    INNER JOIN Purchase.PurchaseReturnHeader prh WITH (NOLOCK)
                        ON prh.Id = prd.PurchaseReturnHeaderId AND prh.IsDeleted = 0
                    WHERE prd.GrnDetailId = gd.Id AND prd.IsDeleted = 0
                ), 0) AS PriorReturnedQty,
                ISNULL(gd.QcAcceptedQuantity, 0)
                    - ISNULL((
                        SELECT SUM(prd.ReturnQty)
                        FROM Purchase.PurchaseReturnDetail prd WITH (NOLOCK)
                        INNER JOIN Purchase.PurchaseReturnHeader prh WITH (NOLOCK)
                            ON prh.Id = prd.PurchaseReturnHeaderId AND prh.IsDeleted = 0
                        WHERE prd.GrnDetailId = gd.Id AND prd.IsDeleted = 0
                    ), 0) AS ReturnableQty,
                gd.UnitPrice AS RatePerUnit
            FROM Purchase.GrnDetail gd WITH (NOLOCK)
            WHERE gd.GrnId = @grnHeaderId;";

        var rows = (await _db.QueryAsync<ReturnableQtyDto>(
            new CommandDefinition(sql, new { grnHeaderId }, cancellationToken: ct))).AsList();

        if (rows.Count > 0)
        {
            var itemIds = rows.Select(r => r.ItemId).Distinct().ToList();
            var uomIds = rows.Select(r => r.UomId).Where(x => x > 0).Distinct().ToList();

            var items = await _itemLookup.GetByIdsAsync(itemIds, ct);
            var itemMap = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

            var uoms = uomIds.Count > 0
                ? await _uomLookup.GetByIdsAsync(uomIds, ct)
                : new List<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>();
            var uomMap = uoms.ToDictionary(u => u.Id, u => u.UOMName);

            foreach (var r in rows)
            {
                if (itemMap.TryGetValue(r.ItemId, out var im))
                {
                    r.ItemCode = im.ItemCode;
                    r.ItemName = im.ItemName;
                }
                if (uomMap.TryGetValue(r.UomId, out var uname))
                    r.UomName = uname;
            }
        }

        return rows;
    }

    public async Task<IReadOnlyList<PurchaseReturnPoLookupDto>> GetPosByVendorAsync(int vendorId, CancellationToken ct)
    {
        // POs for this vendor that have at least one GRN (goods received → returnable).
        // GrnHeader.PartyId is the vendor; same-schema joins, so no cross-module lookup needed.
        const string sql = @"
            SELECT DISTINCT po.Id AS PoId, po.PONumber
            FROM Purchase.GrnHeader gh WITH (NOLOCK)
            JOIN Purchase.GrnDetail gd WITH (NOLOCK) ON gd.GrnId = gh.Id
            JOIN Purchase.PurchaseOrderHeader po WITH (NOLOCK) ON po.Id = gd.PoId
            WHERE gh.PartyId = @vendorId
            ORDER BY po.Id DESC;";

        var rows = (await _db.QueryAsync<PurchaseReturnPoLookupDto>(
            new CommandDefinition(sql, new { vendorId }, cancellationToken: ct))).AsList();
        return rows;
    }

    public async Task<IReadOnlyList<PurchaseReturnGrnLookupDto>> GetGrnsByVendorPoAsync(int vendorId, int poId, CancellationToken ct)
    {
        // GRNs for this vendor + PO (goods received → returnable). Same-schema join, no cross-module lookup.
        const string sql = @"
            SELECT DISTINCT gh.Id AS GrnHeaderId, gh.GrnNo, gh.GrnDate
            FROM Purchase.GrnHeader gh WITH (NOLOCK)
            JOIN Purchase.GrnDetail gd WITH (NOLOCK) ON gd.GrnId = gh.Id
            WHERE gh.PartyId = @vendorId AND gd.PoId = @poId
            ORDER BY gh.Id DESC;";

        var rows = (await _db.QueryAsync<PurchaseReturnGrnLookupDto>(
            new CommandDefinition(sql, new { vendorId, poId }, cancellationToken: ct))).AsList();
        return rows;
    }

    public async Task<IReadOnlyList<PurchaseReturnPendingDto>> GetPendingAsync(int page, int size, string? search, CancellationToken ct)
    {
        page = page <= 0 ? 1 : page;
        size = size <= 0 ? 20 : size;
        var off = (page - 1) * size;

        // RTVs awaiting approval (status = Pending). Approver filtering is done in the handler
        // against AppData.ApprovalRequest via IWorkflowLookup.
        const string sql = @"
            DECLARE @s NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@search)), '');

            SELECT
                h.Id, h.RtvNumber, h.RtvDate,
                h.VendorId,
                h.PoId, po.PONumber AS PoNumber,
                h.GrnHeaderId, gh.GrnNo,
                h.ReturnTypeId, rt.Code AS ReturnTypeCode,
                h.StatusId, mStatus.Code AS StatusCode
            FROM Purchase.PurchaseReturnHeader h WITH (NOLOCK)
            LEFT JOIN Purchase.PurchaseOrderHeader po WITH (NOLOCK) ON po.Id = h.PoId
            LEFT JOIN Purchase.GrnHeader gh WITH (NOLOCK) ON gh.Id = h.GrnHeaderId
            LEFT JOIN Purchase.ReturnType rt WITH (NOLOCK) ON rt.Id = h.ReturnTypeId
            INNER JOIN Purchase.MiscMaster mStatus WITH (NOLOCK) ON mStatus.Id = h.StatusId
            WHERE h.IsDeleted = 0
              AND mStatus.Code = 'Pending'
              AND (@s IS NULL OR h.RtvNumber LIKE '%' + @s + '%')
            ORDER BY h.Id DESC
            OFFSET @off ROWS FETCH NEXT @size ROWS ONLY;";

        var rows = (await _db.QueryAsync<PurchaseReturnPendingDto>(
            new CommandDefinition(sql, new { search, off, size }, cancellationToken: ct))).AsList();

        if (rows.Count > 0)
        {
            var vendorIds = rows.Select(r => r.VendorId).Where(x => x > 0).Distinct().ToList();
            var vendors = await _partyLookup.GetByIdsAsync(vendorIds, ct);
            var vendorMap = vendors.ToDictionary(v => v.Id, v => v.PartyName ?? string.Empty);
            foreach (var r in rows)
                if (vendorMap.TryGetValue(r.VendorId, out var name))
                    r.VendorName = name;
        }

        return rows;
    }

    public async Task<bool> NotFoundAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM Purchase.PurchaseReturnHeader WHERE Id = @id AND IsDeleted = 0;";
        var count = await _db.ExecuteScalarAsync<int>(sql, new { id });
        return count == 0;
    }

    public async Task<int?> GetStatusIdByCodeAsync(string statusCode)
    {
        // Purchase Return uses the shared 'ApprovalStatus' MiscType (Pending/Approved/Rejected/Cancelled)
        // so its StatusId values match the rest of the Purchase modules (PO, Indent, etc.).
        const string sql = @"
            SELECT mm.Id
            FROM Purchase.MiscMaster mm
            INNER JOIN Purchase.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId
            WHERE mt.MiscTypeCode = 'ApprovalStatus'
              AND mm.Code = @statusCode
              AND mm.IsDeleted = 0 AND mt.IsDeleted = 0;";
        return await _db.ExecuteScalarAsync<int?>(sql, new { statusCode });
    }

    public async Task<string?> GetCurrentStatusCodeAsync(int id)
    {
        const string sql = @"
            SELECT mm.Code
            FROM Purchase.PurchaseReturnHeader h
            LEFT JOIN Purchase.MiscMaster mm ON mm.Id = h.StatusId
            WHERE h.Id = @id AND h.IsDeleted = 0;";
        return await _db.ExecuteScalarAsync<string?>(sql, new { id });
    }

    public async Task<string?> GetReturnTypeApprovalRoleCodeAsync(int returnTypeId)
    {
        const string sql = @"
            SELECT ApprovalRoleCode
            FROM Purchase.ReturnType
            WHERE Id = @id AND IsDeleted = 0;";
        return await _db.ExecuteScalarAsync<string?>(sql, new { id = returnTypeId });
    }
}
