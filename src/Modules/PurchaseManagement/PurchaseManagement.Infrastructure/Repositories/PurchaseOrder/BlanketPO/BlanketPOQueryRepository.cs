using System.Data;
using Contracts.Interfaces;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBlanketPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.BlanketPO;

public sealed class BlanketPOQueryRepository : IBlanketPOQueryRepository
{
    private readonly IDbConnection _db;
    private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
    private readonly IIPAddressService _ipAddressService;

    public BlanketPOQueryRepository(
        IDbConnection db,
        IMiscMasterQueryRepository miscMasterQueryRepository,
        IIPAddressService ipAddressService)
    {
        _db = db;
        _miscMasterQueryRepository = miscMasterQueryRepository;
        _ipAddressService = ipAddressService;
    }

    public async Task<decimal> GetBlanketDetailBalanceAsync(int blanketDetailId)
    {
        const string sql = @"
            SELECT ISNULL(bd.EstimatedQuantity, 0) - ISNULL(SUM(pbd.Quantity), 0)
            FROM Purchase.BlanketDetail bd
            LEFT JOIN Purchase.PurchaseBlanketDetail pbd
                ON pbd.BlanketDetailId = bd.Id AND pbd.IsDeleted = 0
            LEFT JOIN Purchase.PurchaseBlanketHeader pbh
                ON pbd.PurchaseBlanketHeaderId = pbh.Id AND pbh.IsDeleted = 0
            LEFT JOIN Purchase.PurchaseOrderHeader poh
                ON pbh.PurchaseOrderId = poh.Id AND poh.IsDeleted = 0
            WHERE bd.Id = @Id AND bd.IsDeleted = 0
            GROUP BY bd.EstimatedQuantity";

        var result = await _db.ExecuteScalarAsync<decimal?>(sql, new { Id = blanketDetailId });
        return result ?? 0;
    }

    public async Task<bool> HasAnyGrnAsync(int poId, CancellationToken ct)
    {
        const string sql = @"SELECT TOP 1 1 FROM Purchase.GRNDetail g WHERE g.PoId = @poId;";
        var exists = await _db.ExecuteScalarAsync<int?>(sql, new { poId });
        return exists.HasValue;
    }

    public async Task<BlanketPODetailVm?> GetBlanketPOByIdAsync(int poId, CancellationToken ct)
    {
        const string headerSql = @"
            SELECT
                H.Id,
                H.UnitId,
                H.PONumber,
                H.PODate,
                H.POCategoryId,
                H.POMethodId,
                H.StatusId,
                H.ItemTotal,
                H.DiscountTotal,
                H.PandFTotal,
                H.MiscCharges,
                H.GSTTotal,
                H.CGSTTotal,
                H.SGSTTotal,
                H.IGSTTotal,
                H.FreightTotal,
                H.PurchaseValue,
                H.RevisionNo,
                H.VendorId,
                H.CurrencyId,
                H.CostCenterId,
                H.BudgetGroupId,
                H.BudgetMonthId,
                H.BudgetRequestById,
                H.ProjectId,
                H.WBSId,
                H.FinancialYearId,
                BH.BlanketHeaderId,
                BH.IsPartialReceiptAllowed,
                BH.IncotermsId,
                BH.ModeOfDispatchId,
                BH.FreightCharges,
                BH.TermsId,
                BH.TermDescription,
                BH.DeliveryAddress,
                BH.BillingAddress,
                BMH.BlanketNumber
            FROM Purchase.PurchaseOrderHeader H WITH (NOLOCK)
            INNER JOIN Purchase.PurchaseBlanketHeader BH WITH (NOLOCK)
                ON BH.PurchaseOrderId = H.Id AND BH.IsDeleted = 0
            LEFT JOIN Purchase.BlanketHeader BMH WITH (NOLOCK)
                ON BMH.Id = BH.BlanketHeaderId AND BMH.IsDeleted = 0
            WHERE H.Id = @Id AND H.IsDeleted = 0";

        const string detailSql = @"
            SELECT
                BD.BlanketDetailId,
                BD.ItemSno,
                BD.ItemId,
                BD.UOMId,
                BD.Quantity,
                BD.UnitPrice,
                BD.ItemValue,
                BD.DiscountTypeId,
                BD.DiscountValue,
                BD.PandFType,
                BD.PandFCharge,
                BD.OtherCharge,
                BD.GSTPercentage,
                BD.CGSTPercentage,
                BD.SGSTPercentage,
                BD.IGSTPercentage,
                BD.CGST,
                BD.SGST,
                BD.IGST,
                BD.ScheduleDate,
                BD.DepartmentId
            FROM Purchase.PurchaseBlanketDetail BD WITH (NOLOCK)
            INNER JOIN Purchase.PurchaseBlanketHeader BH WITH (NOLOCK)
                ON BH.Id = BD.PurchaseBlanketHeaderId AND BH.IsDeleted = 0
            WHERE BH.PurchaseOrderId = @Id AND BD.IsDeleted = 0
            ORDER BY BD.ItemSno";

        const string paymentTermSql = @"
            SELECT
                PT.Id,
                PT.PurchaseOrderId,
                PT.PaymentTermId,
                PT.AdvancePercent,
                PT.CreditDays,
                PT.PaymentModelId AS PaymentModeId,
                PT.InsuranceId,
                PT.InsurancePercent,
                PT.InsuranceAmount,
                PT.AdvanceAmount,
                PT.BalancePercent,
                PT.BalanceAmount
            FROM Purchase.PurchasePaymentTerm PT WITH (NOLOCK)
            WHERE PT.PurchaseOrderId = @Id AND PT.IsDeleted = 0";

        var headerCmd = new CommandDefinition(headerSql, new { Id = poId }, cancellationToken: ct);
        var vm = await _db.QueryFirstOrDefaultAsync<BlanketPODetailVm>(headerCmd);
        if (vm is null) return null;

        var detailCmd = new CommandDefinition(detailSql, new { Id = poId }, cancellationToken: ct);
        var details = (await _db.QueryAsync<BlanketPODetailItem>(detailCmd)).AsList();
        vm.Details = details;

        var ptCmd = new CommandDefinition(paymentTermSql, new { Id = poId }, cancellationToken: ct);
        var terms = (await _db.QueryAsync<BlanketPOPaymentTermItem>(ptCmd)).AsList();
        vm.PaymentTerms = terms;

        return vm;
    }

    public async Task<(List<GetBlanketPOPendingGroupDto> Rows, int Total)> GetBlanketPOPendingAsync(
        int? page, int? size, string? search, int? poId, CancellationToken ct)
    {
        var p = (page.HasValue && page > 0) ? page.Value : 1;
        var s = (size.HasValue && size > 0) ? size.Value : 15;
        var off = (p - 1) * s;
        var like = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
        var unitId = _ipAddressService.GetUnitId() ?? 0;

        // Only "Pending" status
        var pending = await _miscMasterQueryRepository.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

        const string sql = @"
            /* 1) Matching Blanket Release PO ids */
            CREATE TABLE #filtered (Id INT PRIMARY KEY);

            INSERT INTO #filtered (Id)
            SELECT DISTINCT h.Id
            FROM Purchase.PurchaseOrderHeader h WITH (NOLOCK)
            INNER JOIN Purchase.PurchaseBlanketHeader bh WITH (NOLOCK)
                ON bh.PurchaseOrderId = h.Id AND bh.IsDeleted = 0
            WHERE h.IsDeleted = 0
              AND (@UnitId IS NULL OR h.UnitId = @UnitId)
              AND h.StatusId = @StatusId
              AND (@PoId IS NULL OR h.Id = @PoId)
              AND (@LikeSearch IS NULL OR h.PONumber LIKE @LikeSearch);

            /* 2) Paged ids */
            CREATE TABLE #paged (Id INT PRIMARY KEY);
            WITH numbered AS (
                SELECT Id, ROW_NUMBER() OVER (ORDER BY Id DESC) rn
                FROM #filtered
            )
            INSERT INTO #paged (Id)
            SELECT Id FROM numbered WHERE rn BETWEEN (@off + 1) AND (@off + @size);

            /* 3) Header groups */
            SELECT
                h.Id,
                h.PONumber,
                h.PODate,
                h.VendorId,
                h.PurchaseValue,
                h.StatusId,
                h.ItemTotal,
                h.DiscountTotal,
                h.PandFTotal,
                h.MiscCharges,
                h.GSTTotal,
                h.CGSTTotal,
                h.SGSTTotal,
                h.IGSTTotal,
                h.FreightTotal,
                h.CreatedDate      AS createdDate,
                h.CreatedByName    AS createdByName,
                st.Code            AS StatusCode,
                cat.Code           AS POCategoryCode,
                mth.Code           AS POMethodCode,
                mth.Id             AS POMethodId,
                bh.BlanketHeaderId,
                bmh.BlanketNumber
            FROM Purchase.PurchaseOrderHeader h WITH (NOLOCK)
            INNER JOIN Purchase.PurchaseBlanketHeader bh WITH (NOLOCK)
                ON bh.PurchaseOrderId = h.Id AND bh.IsDeleted = 0
            LEFT JOIN Purchase.BlanketHeader bmh WITH (NOLOCK)
                ON bmh.Id = bh.BlanketHeaderId AND bmh.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster st  WITH (NOLOCK) ON st.Id  = h.StatusId
            LEFT JOIN Purchase.MiscMaster cat WITH (NOLOCK) ON cat.Id = h.POCategoryId
            LEFT JOIN Purchase.MiscMaster mth WITH (NOLOCK) ON mth.Id = h.POMethodId
            WHERE h.Id IN (SELECT Id FROM #paged)
            ORDER BY h.Id DESC;

            /* 4) Blanket details */
            SELECT
                bd.Id,
                bd.PurchaseBlanketHeaderId,
                bd.BlanketDetailId,
                bd.ItemId,
                bd.Quantity,
                bd.UnitPrice,
                bd.DiscountTypeId,
                bd.DiscountValue,
                bd.PandFType,
                bd.PandFCharge,
                bd.OtherCharge,
                bd.GSTPercentage,
                bd.CGSTPercentage,
                bd.SGSTPercentage,
                bd.IGSTPercentage,
                bd.CGST,
                bd.SGST,
                bd.IGST,
                bd.ScheduleDate,
                bd.DepartmentId,
                bd.ItemValue
            FROM Purchase.PurchaseBlanketDetail bd WITH (NOLOCK)
            INNER JOIN Purchase.PurchaseBlanketHeader bh WITH (NOLOCK)
                ON bh.Id = bd.PurchaseBlanketHeaderId AND bh.IsDeleted = 0
            WHERE bd.IsDeleted = 0
              AND bh.PurchaseOrderId IN (SELECT Id FROM #paged)
            ORDER BY bd.PurchaseBlanketHeaderId, bd.ItemSno;

            /* 4b) Map PurchaseBlanketHeaderId -> PurchaseOrderId */
            SELECT bh.Id AS PurchaseBlanketHeaderId, bh.PurchaseOrderId
            FROM Purchase.PurchaseBlanketHeader bh WITH (NOLOCK)
            WHERE bh.IsDeleted = 0
              AND bh.PurchaseOrderId IN (SELECT Id FROM #paged);

            /* 5) Total */
            SELECT COUNT(1) FROM #filtered;

            /* 6) Cleanup */
            DROP TABLE #paged;
            DROP TABLE #filtered;";

        var param = new
        {
            UnitId = unitId,
            StatusId = pending.Id,
            PoId = poId,
            LikeSearch = like,
            off,
            size = s
        };

        using var multi = await _db.QueryMultipleAsync(
            new CommandDefinition(sql, param, cancellationToken: ct));

        // 3) headers
        var headers = (await multi.ReadAsync<GetBlanketPOPendingGroupDto>()).ToList();

        // 4) details
        var details = (await multi.ReadAsync<GetBlanketPOPendingDto>()).ToList();

        // 4b) blanket header -> PO id map
        var mapRows = (await multi.ReadAsync<(int PurchaseBlanketHeaderId, int PurchaseOrderId)>()).ToList();
        var poByBlanketHeaderId = mapRows
            .GroupBy(x => x.PurchaseBlanketHeaderId)
            .ToDictionary(g => g.Key, g => g.First().PurchaseOrderId);

        // 5) total
        var total = await multi.ReadFirstAsync<int>();

        // Attach details to headers
        var byPo = headers.ToDictionary(h => h.Id, h => h);
        foreach (var g in headers) g.Lines ??= new List<GetBlanketPOPendingDto>();

        foreach (var d in details)
        {
            if (poByBlanketHeaderId.TryGetValue(d.PurchaseBlanketHeaderId, out var poId2)
                && byPo.TryGetValue(poId2, out var grp))
            {
                grp.Lines.Add(d);
            }
        }

        return (headers, total);
    }

    public async Task<bool> NotFoundAsync(int poId, CancellationToken ct)
    {
        const string sql = @"
            SELECT CASE WHEN NOT EXISTS (
                SELECT 1 FROM Purchase.PurchaseOrderHeader WITH (NOLOCK)
                WHERE Id = @Id AND IsDeleted = 0
            ) THEN 1 ELSE 0 END";

        return await _db.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { Id = poId }, cancellationToken: ct));
    }
}
