using System.Data;
using Contracts.Interfaces;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Queries.GetContractPOPending;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ContractPO;

public sealed class ContractPOQueryRepository : IContractPOQueryRepository
{
    private readonly IDbConnection _db;
    private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
    private readonly IIPAddressService _ipAddressService;

    public ContractPOQueryRepository(
        IDbConnection db,
        IMiscMasterQueryRepository miscMasterQueryRepository,
        IIPAddressService ipAddressService)
    {
        _db = db;
        _miscMasterQueryRepository = miscMasterQueryRepository;
        _ipAddressService = ipAddressService;
    }

    public async Task<decimal> GetContractDetailBalanceAsync(int contractPODetailId)
    {
        const string sql = @"
            SELECT ISNULL(BalanceQuantity, 0)
            FROM Purchase.ContractPODetail
            WHERE Id = @Id AND IsDeleted = 0";

        return await _db.ExecuteScalarAsync<decimal>(sql, new { Id = contractPODetailId });
    }

    public async Task<bool> HasAnyGrnAsync(int poId, CancellationToken ct)
    {
        const string sql = @"SELECT TOP 1 1 FROM Purchase.GRNDetail g WHERE g.PoId = @poId;";
        var exists = await _db.ExecuteScalarAsync<int?>(sql, new { poId });
        return exists.HasValue;
    }

    public async Task<ContractPODetailVm?> GetContractPOByIdAsync(int poId, CancellationToken ct)
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
                CH.ContractPOHeaderId,
                CH.IsPartialReceiptAllowed,
                CH.IncotermsId,
                CH.ModeOfDispatchId,
                CH.FreightCharges,
                CH.TermsId,
                CH.TermDescription,
                CH.DeliveryAddress,
                CH.BillingAddress,
                CPH.ContractPONumber
            FROM Purchase.PurchaseOrderHeader H WITH (NOLOCK)
            INNER JOIN Purchase.PurchaseContractHeader CH WITH (NOLOCK)
                ON CH.PurchaseOrderId = H.Id AND CH.IsDeleted = 0
            LEFT JOIN Purchase.ContractPOHeader CPH WITH (NOLOCK)
                ON CPH.Id = CH.ContractPOHeaderId AND CPH.IsDeleted = 0
            WHERE H.Id = @Id AND H.IsDeleted = 0";

        const string detailSql = @"
            SELECT
                CD.ContractPODetailId,
                CD.ItemSno,
                CD.ItemId,
                CD.UOMId,
                CD.Quantity,
                CD.UnitPrice,
                CD.ItemValue,
                CD.DiscountTypeId,
                CD.DiscountValue,
                CD.PandFType,
                CD.PandFCharge,
                CD.OtherCharge,
                CD.GSTPercentage,
                CD.CGSTPercentage,
                CD.SGSTPercentage,
                CD.IGSTPercentage,
                CD.CGST,
                CD.SGST,
                CD.IGST,
                CD.ScheduleDate,
                CD.DepartmentId
            FROM Purchase.PurchaseContractDetail CD WITH (NOLOCK)
            INNER JOIN Purchase.PurchaseContractHeader CH WITH (NOLOCK)
                ON CH.Id = CD.PurchaseContractHeaderId AND CH.IsDeleted = 0
            WHERE CH.PurchaseOrderId = @Id AND CD.IsDeleted = 0
            ORDER BY CD.ItemSno";

        const string paymentTermSql = @"
            SELECT
                PT.Id,
                PT.PurchaseOrderId,
                PT.PaymentTermId,
                PT.AdvancePercent,
                PT.CreditDays,
                PT.PaymentModelId,
                PT.InsuranceId,
                PT.InsurancePercent,
                PT.InsuranceAmount,
                PT.AdvanceAmount,
                PT.BalancePercent,
                PT.BalanceAmount
            FROM Purchase.PurchasePaymentTerm PT WITH (NOLOCK)
            WHERE PT.PurchaseOrderId = @Id AND PT.IsDeleted = 0";

        var headerCmd = new CommandDefinition(headerSql, new { Id = poId }, cancellationToken: ct);
        var vm = await _db.QueryFirstOrDefaultAsync<ContractPODetailVm>(headerCmd);
        if (vm is null) return null;

        var detailCmd = new CommandDefinition(detailSql, new { Id = poId }, cancellationToken: ct);
        var details = (await _db.QueryAsync<ContractPODetailItem>(detailCmd)).AsList();
        vm.Details = details;

        var ptCmd = new CommandDefinition(paymentTermSql, new { Id = poId }, cancellationToken: ct);
        var terms = (await _db.QueryAsync<ContractPOPaymentTermItem>(ptCmd)).AsList();
        vm.PaymentTerms = terms;

        return vm;
    }

    public async Task<(List<GetContractPOPendingGroupDto> Rows, int Total)> GetContractPOPendingAsync(
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
            /* 1) Matching Contract Release PO ids */
            CREATE TABLE #filtered (Id INT PRIMARY KEY);

            INSERT INTO #filtered (Id)
            SELECT DISTINCT h.Id
            FROM Purchase.PurchaseOrderHeader h WITH (NOLOCK)
            INNER JOIN Purchase.PurchaseContractHeader ch WITH (NOLOCK)
                ON ch.PurchaseOrderId = h.Id AND ch.IsDeleted = 0
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
                h.InsuranceTotal,
                h.TDSTotal,
                h.AdvanceAmount,
                h.CreatedDate      AS createdDate,
                h.CreatedByName    AS createdByName,
                st.Code            AS StatusCode,
                cat.Code           AS POCategoryCode,
                mth.Code           AS POMethodCode,
                mth.Id             AS POMethodId,
                ch.ContractPOHeaderId,
                cph.ContractPONumber
            FROM Purchase.PurchaseOrderHeader h WITH (NOLOCK)
            INNER JOIN Purchase.PurchaseContractHeader ch WITH (NOLOCK)
                ON ch.PurchaseOrderId = h.Id AND ch.IsDeleted = 0
            LEFT JOIN Purchase.ContractPOHeader cph WITH (NOLOCK)
                ON cph.Id = ch.ContractPOHeaderId AND cph.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster st  WITH (NOLOCK) ON st.Id  = h.StatusId
            LEFT JOIN Purchase.MiscMaster cat WITH (NOLOCK) ON cat.Id = h.POCategoryId
            LEFT JOIN Purchase.MiscMaster mth WITH (NOLOCK) ON mth.Id = h.POMethodId
            WHERE h.Id IN (SELECT Id FROM #paged)
            ORDER BY h.Id DESC;

            /* 4) Contract details */
            SELECT
                cd.Id,
                cd.PurchaseContractHeaderId,
                cd.ContractPODetailId,
                cd.ItemId,
                cd.Quantity,
                cd.UnitPrice,
                cd.DiscountTypeId,
                cd.DiscountValue,
                cd.PandFType,
                cd.PandFCharge,
                cd.OtherCharge,
                cd.GSTPercentage,
                cd.CGSTPercentage,
                cd.SGSTPercentage,
                cd.IGSTPercentage,
                cd.CGST,
                cd.SGST,
                cd.IGST,
                cd.ScheduleDate,
                cd.DepartmentId,
                cd.ItemValue
            FROM Purchase.PurchaseContractDetail cd WITH (NOLOCK)
            INNER JOIN Purchase.PurchaseContractHeader ch WITH (NOLOCK)
                ON ch.Id = cd.PurchaseContractHeaderId AND ch.IsDeleted = 0
            WHERE cd.IsDeleted = 0
              AND ch.PurchaseOrderId IN (SELECT Id FROM #paged)
            ORDER BY cd.PurchaseContractHeaderId, cd.ItemSno;

            /* 4b) Map PurchaseContractHeaderId -> PurchaseOrderId */
            SELECT ch.Id AS PurchaseContractHeaderId, ch.PurchaseOrderId
            FROM Purchase.PurchaseContractHeader ch WITH (NOLOCK)
            WHERE ch.IsDeleted = 0
              AND ch.PurchaseOrderId IN (SELECT Id FROM #paged);

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
        var headers = (await multi.ReadAsync<GetContractPOPendingGroupDto>()).ToList();

        // 4) details
        var details = (await multi.ReadAsync<GetContractPOPendingDto>()).ToList();

        // 4b) contract header -> PO id map
        var mapRows = (await multi.ReadAsync<(int PurchaseContractHeaderId, int PurchaseOrderId)>()).ToList();
        var poByContractHeaderId = mapRows
            .GroupBy(x => x.PurchaseContractHeaderId)
            .ToDictionary(g => g.Key, g => g.First().PurchaseOrderId);

        // 5) total
        var total = await multi.ReadFirstAsync<int>();

        // Attach details to headers
        var byPo = headers.ToDictionary(h => h.Id, h => h);
        foreach (var g in headers) g.Lines ??= new List<GetContractPOPendingDto>();

        foreach (var d in details)
        {
            if (poByContractHeaderId.TryGetValue(d.PurchaseContractHeaderId, out var poId2)
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
