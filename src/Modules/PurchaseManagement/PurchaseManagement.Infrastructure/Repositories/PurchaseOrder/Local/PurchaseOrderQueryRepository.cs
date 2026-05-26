using System.Data;
using PurchaseManagement.Application.Common;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPOLocalPending;
using PurchaseManagement.Domain.Common;
using Dapper;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.Local;

public class PurchaseOrderQueryRepository : IPurchaseOrderQueryRepository
{
    private readonly IDbConnection _conn;
    private readonly IIPAddressService _ip;
    private readonly ApplicationDbContext _applicationDb;
    private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

    public PurchaseOrderQueryRepository(
        IDbConnection conn,
        IIPAddressService ip,
        ApplicationDbContext applicationDb,
        IMiscMasterQueryRepository miscMasterQueryRepository)
    {
        _conn = conn;
        _ip = ip;
        _applicationDb = applicationDb;
        _miscMasterQueryRepository = miscMasterQueryRepository;
    }

     public async Task<PagedResult<PurchaseOrderListItemDto>> GetAllAsync(
        int page, 
        int size, 
        string? searchTerm, 
        int? poMethodId, 
        int? statusId, 
        int? budgetGroupId,
        CancellationToken ct)
    {
        const string sql = @"
        -- Count
        SELECT COUNT(1)
        FROM Purchase.PurchaseOrderHeader h WITH (NOLOCK)
        WHERE h.IsDeleted = 0
          AND h.IsActive  = 1
          AND h.UnitId    = @UnitId
          AND (@Search IS NULL OR @Search = '' OR h.PONumber LIKE @LikeSearch )
          AND (@PoMethodId IS NULL OR h.POMethodId = @PoMethodId)
          AND (@BudgetGroupId IS NULL OR h.BudgetGroupId = @BudgetGroupId)
          AND (@StatusId IS NULL OR h.StatusId = @StatusId);

        -- Page
          SELECT
           h.Id, h.PONumber, h.PODate, h.VendorId, h.StatusId,
            h.PurchaseValue, h.ItemTotal, h.DiscountTotal, h.PandFTotal, h.MiscCharges,
            h.GSTTotal, h.CGSTTotal, h.SGSTTotal, h.IGSTTotal, h.FreightTotal,
            h.InsuranceTotal, h.TDSTotal, h.AdvanceAmount,
            mStatus.Code AS StatusCode,
            mCat.Code    AS POCategoryCode,
            mMethod.Code AS POMethodCode, mMethod.id as POMethodId,h.BudgetGroupId,h.ItemCategoryId,
            CASE 
                WHEN x.HasGRN  = 1 THEN 2
                WHEN x.HasGate = 1 THEN 2
                WHEN mStatus.Code = @Approved THEN 1
                WHEN mStatus.Code = @Pending  THEN 0
                ELSE 0
            END AS Edit,
            CASE 
                WHEN x.HasGRN  = 1 THEN N'GRN exists for this PO. Edit/Amendment is not allowed'
                WHEN x.HasGate = 1 THEN N'Gate Entry exists for this PO. Edit/Amendment is not allowed'
                WHEN mStatus.Code = @Approved THEN N'This PO is approved; editing it will create an amendment and assign a new PO number with a revision code.'
                WHEN mStatus.Code = @Pending  THEN NULL
            END AS EditReason,
            h.RevisionNo,
            h.AmendmentReason,
            CASE
                WHEN LOWER(mStatus.Code) = LOWER(@Approved)
                THEN CASE WHEN x.HasGRN = 1 THEN 'Y' ELSE 'N' END
                ELSE NULL
            END AS GRNFlag,
            CAST(CASE WHEN mStatus.Code = @Cancelled THEN 1 ELSE 0 END AS BIT) AS IsCancelled,
            CAST(CASE WHEN mStatus.Code = @ForeClosed THEN 1 ELSE 0 END AS BIT) AS IsForeclosed,
            CAST(CASE WHEN x.HasGRN = 0 AND mStatus.Code = @Approved THEN 1 ELSE 0 END AS BIT) AS CanCancel,
            CAST(CASE WHEN x.HasGRN = 1 AND mStatus.Code = @Approved THEN 1 ELSE 0 END AS BIT) AS CanForeclose,
            h.CancelledDate,
            h.CancelledByName,
            h.CancelledIP,
            h.ForeClosedDate,
            h.ForeClosedByName,
            h.ForeClosedIP
        FROM Purchase.PurchaseOrderHeader h WITH (NOLOCK)
        LEFT JOIN Purchase.MiscMaster mStatus WITH (NOLOCK) ON mStatus.Id = h.StatusId
        LEFT JOIN Purchase.MiscMaster mCat    WITH (NOLOCK) ON mCat.Id    = h.POCategoryId        
		 LEFT JOIN Purchase.MiscMaster mMethod WITH (NOLOCK) ON mMethod.Id = h.POMethodId
        OUTER APPLY (
            SELECT
                CASE WHEN EXISTS (SELECT 1 FROM Purchase.GrnDetail       gd WITH (NOLOCK) WHERE gd.PoId = h.Id) THEN 1 ELSE 0 END AS HasGRN,
                CASE WHEN EXISTS (SELECT 1 FROM Purchase.GateEntryDetail ge WITH (NOLOCK) WHERE ge.PoId = h.Id) THEN 1 ELSE 0 END AS HasGate
        ) x
        WHERE h.IsDeleted = 0
          AND h.IsActive  = 1
          AND h.UnitId    = @UnitId        
          AND (@PoMethodId IS NULL OR h.POMethodId = @PoMethodId)
          AND (@BudgetGroupId IS NULL OR h.BudgetGroupId = @BudgetGroupId)
          AND (@StatusId IS NULL OR h.StatusId = @StatusId)
        ORDER BY h.Id DESC
        OFFSET @off ROWS FETCH NEXT @size ROWS ONLY;";

        var like = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm.Trim()}%";

        using var multi = await _conn.QueryMultipleAsync(sql, new
        {
            UnitId = _ip.GetUnitId() ?? 0,
            Search = searchTerm,
            LikeSearch = like,  
            PoMethodId = poMethodId,
            BudgetGroupId = budgetGroupId,
            off = Math.Max(0, (page - 1) * size),
            size,
            Pending = MiscEnumEntity.Pending,
            Approved = MiscEnumEntity.Approved,
            Cancelled = MiscEnumEntity.Cancelled,
            ForeClosed = MiscEnumEntity.ForeClosed,
            StatusId = statusId
        });

        var total = await multi.ReadFirstAsync<int>();
        var items = (await multi.ReadAsync<PurchaseOrderListItemDto>()).ToList();

        return new PagedResult<PurchaseOrderListItemDto>
        {
            Page = page,
            PageSize = size,
            Total = total,
            Items = items,
        };
    }
    public async Task<PagedResult<PurchaseOrderListItemDto>> GetMyPurchaseOrdersAsync(
        int vendorPartyId,
        int page,
        int size,
        string? searchTerm,
        int? poMethodId,
        int? statusId,
        int? budgetGroupId,
        CancellationToken ct)
    {
        const string sql = @"
        -- Count
        SELECT COUNT(1)
        FROM Purchase.PurchaseOrderHeader h WITH (NOLOCK)
        WHERE h.IsDeleted = 0
          AND h.IsActive  = 1
          AND h.VendorId  = @VendorId
          AND (@Search IS NULL OR @Search = '' OR h.PONumber LIKE @LikeSearch )
          AND (@PoMethodId IS NULL OR h.POMethodId = @PoMethodId)
          AND (@BudgetGroupId IS NULL OR h.BudgetGroupId = @BudgetGroupId)
          AND (@StatusId IS NULL OR h.StatusId = @StatusId);

        -- Page
          SELECT
           h.Id, h.PONumber, h.PODate, h.VendorId, h.StatusId,
            h.PurchaseValue, h.ItemTotal, h.DiscountTotal, h.PandFTotal, h.MiscCharges,
            h.GSTTotal, h.CGSTTotal, h.SGSTTotal, h.IGSTTotal, h.FreightTotal,
            h.InsuranceTotal, h.TDSTotal, h.AdvanceAmount,
            mStatus.Code AS StatusCode,
            mCat.Code    AS POCategoryCode,
            mMethod.Code AS POMethodCode, mMethod.id as POMethodId,h.BudgetGroupId,h.ItemCategoryId,
            CASE
                WHEN x.HasGRN  = 1 THEN 2
                WHEN x.HasGate = 1 THEN 2
                WHEN mStatus.Code = @Approved THEN 1
                WHEN mStatus.Code = @Pending  THEN 0
                ELSE 0
            END AS Edit,
            CASE
                WHEN x.HasGRN  = 1 THEN N'GRN exists for this PO. Edit/Amendment is not allowed'
                WHEN x.HasGate = 1 THEN N'Gate Entry exists for this PO. Edit/Amendment is not allowed'
                WHEN mStatus.Code = @Approved THEN N'This PO is approved; editing it will create an amendment and assign a new PO number with a revision code.'
                WHEN mStatus.Code = @Pending  THEN NULL
            END AS EditReason,
            h.RevisionNo,
            h.AmendmentReason,
            CASE
                WHEN LOWER(mStatus.Code) = LOWER(@Approved)
                THEN CASE WHEN x.HasGRN = 1 THEN 'Y' ELSE 'N' END
                ELSE NULL
            END AS GRNFlag,
            CAST(CASE WHEN mStatus.Code = @Cancelled THEN 1 ELSE 0 END AS BIT) AS IsCancelled,
            CAST(CASE WHEN mStatus.Code = @ForeClosed THEN 1 ELSE 0 END AS BIT) AS IsForeclosed,
            CAST(CASE WHEN x.HasGRN = 0 AND mStatus.Code = @Approved THEN 1 ELSE 0 END AS BIT) AS CanCancel,
            CAST(CASE WHEN x.HasGRN = 1 AND mStatus.Code = @Approved THEN 1 ELSE 0 END AS BIT) AS CanForeclose,
            h.CancelledDate,
            h.CancelledByName,
            h.CancelledIP,
            h.ForeClosedDate,
            h.ForeClosedByName,
            h.ForeClosedIP
        FROM Purchase.PurchaseOrderHeader h WITH (NOLOCK)
        LEFT JOIN Purchase.MiscMaster mStatus WITH (NOLOCK) ON mStatus.Id = h.StatusId
        LEFT JOIN Purchase.MiscMaster mCat    WITH (NOLOCK) ON mCat.Id    = h.POCategoryId
         LEFT JOIN Purchase.MiscMaster mMethod WITH (NOLOCK) ON mMethod.Id = h.POMethodId
        OUTER APPLY (
            SELECT
                CASE WHEN EXISTS (SELECT 1 FROM Purchase.GrnDetail       gd WITH (NOLOCK) WHERE gd.PoId = h.Id) THEN 1 ELSE 0 END AS HasGRN,
                CASE WHEN EXISTS (SELECT 1 FROM Purchase.GateEntryDetail ge WITH (NOLOCK) WHERE ge.PoId = h.Id) THEN 1 ELSE 0 END AS HasGate
        ) x
        WHERE h.IsDeleted = 0
          AND h.IsActive  = 1
          AND h.VendorId  = @VendorId
          AND (@PoMethodId IS NULL OR h.POMethodId = @PoMethodId)
          AND (@BudgetGroupId IS NULL OR h.BudgetGroupId = @BudgetGroupId)
          AND (@StatusId IS NULL OR h.StatusId = @StatusId)
        ORDER BY h.Id DESC
        OFFSET @off ROWS FETCH NEXT @size ROWS ONLY;";

        var like = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm.Trim()}%";

        using var multi = await _conn.QueryMultipleAsync(sql, new
        {
            VendorId = vendorPartyId,
            Search = searchTerm,
            LikeSearch = like,
            PoMethodId = poMethodId,
            BudgetGroupId = budgetGroupId,
            off = Math.Max(0, (page - 1) * size),
            size,
            Pending = MiscEnumEntity.Pending,
            Approved = MiscEnumEntity.Approved,
            Cancelled = MiscEnumEntity.Cancelled,
            ForeClosed = MiscEnumEntity.ForeClosed,
            StatusId = statusId
        });

        var total = await multi.ReadFirstAsync<int>();
        var items = (await multi.ReadAsync<PurchaseOrderListItemDto>()).ToList();

        return new PagedResult<PurchaseOrderListItemDto>
        {
            Page = page,
            PageSize = size,
            Total = total,
            Items = items,
        };
    }

    public async Task<PurchaseOrderDetailDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        // header
        var header = await _conn.QueryFirstOrDefaultAsync<PurchaseOrderDetailDto>(
            "SELECT * FROM Purchase.PurchaseOrderHeader WHERE Id=@id AND IsDeleted=0;", new { id });

        if (header is null) return null;

        // payment terms
        var terms = await _conn.QueryAsync<PurchasePaymentTermDto>(
            "SELECT * FROM Purchase.PurchasePaymentTerm WHERE PurchaseOrderId=@id AND IsDeleted=0 ORDER BY Id;", new { id });
        header.PaymentTerms = terms.ToList();

        // documents (raw paths - enrichment done in handler)
        const string docSql = @"
        SELECT
            d.DocumentId,
            mm.Code AS DocumentName,
            d.FileName,
            d.UploadedDate
        FROM Purchase.PurchaseDocuments AS d WITH (NOLOCK)
        JOIN Purchase.MiscMaster AS mm ON mm.Id = d.DocumentId
        WHERE d.POId = @id
        ORDER BY d.Id;";

        var rows = await _conn.QueryAsync<LocalDocumentDto>(docSql, new { id });
        header.DocumentsList = rows.ToList();

        // local headers
        var locals = (await _conn.QueryAsync<PurchaseLocalHeaderDto>(
            @"SELECT lh.*
            FROM Purchase.PurchaseLocalHeader lh
            WHERE lh.PurchaseOrderId = @id AND lh.IsDeleted = 0
            ORDER BY lh.Id;",
            new { id }
        )).ToList();

        header.Headers = locals;

        var hasGrn = await HasAnyGrnAsync(id, ct);
        var statusCode = await GetStatusCodeAsync(id, ct) ?? "";
        var (editFlag, editReason) = ComputeEditGate(hasGrn, statusCode);

        header.Edit = editFlag;
        header.EditReason = editReason;

        if (locals.Count > 0)
        {
            // fetch all details for these headers in one go
            var localIds = locals.Where(l => l.Id.HasValue).Select(l => l.Id!.Value).ToArray();
            var details = (await _conn.QueryAsync<PurchaseLocalDetailDto>(
                "SELECT * FROM Purchase.PurchaseLocalDetail WHERE IsDeleted=0 AND PurchaseLocalId IN @ids ORDER BY Id;",
                new { ids = localIds })).ToList();

            if (details.Count == 0)
                return header;

            // Fetch indent numbers from SQL (local data)
            var indentIds = details.Select(x => x.IndentId ?? 0).Where(x => x > 0).Distinct().ToArray();
            if (indentIds.Length > 0)
            {
                var indentRows = await _conn.QueryAsync(
                    @"SELECT Id, IndentNumber
                    FROM Purchase.IndentHeader
                    WHERE IsDeleted = 0 AND Id IN @ids;",
                    new { ids = indentIds });

                var indentMap = new Dictionary<int, string>();
                foreach (var r in indentRows)
                {
                    int rid = (int)r.Id;
                    string num = (string)r.IndentNumber;
                    indentMap[rid] = num;
                }

                foreach (var d in details)
                {
                    if (d.IndentId is > 0 && indentMap.TryGetValue(d.IndentId.Value, out var indentNo))
                        d.IndentNumber = indentNo;
                }
            }

            var lookup = details.GroupBy(d => d.PurchaseLocalId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var lh in locals)
            {
                if (lh.Id is null) continue;
                if (lookup.TryGetValue(lh.Id.Value, out var list))
                    lh.Details = list;
            }
        }

        return header;
    }
    static (int Edit, string? Reason) ComputeEditGate(bool hasGrn, string statusCode)
    {
        if (hasGrn)
            return (2, "GRN exists for this PO. Edit/Amendment  is not allowed.");

        if (statusCode.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            return (0, null);

        if (statusCode.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            return (1, "This PO is approved; editing it will create an amendment and assign a new PO number with a revision code.");

        // default safe fallback = block
        return (0, $"Editing is allowed'.");
    }

    public Task<IEnumerable<AutocompleteDto>> GetAutocompleteAsync(string? term, int? poMethodId,int? budgetGroupId, CancellationToken ct)
    {
        const string sql = @"
            SELECT h.Id, h.PONumber
            FROM Purchase.PurchaseOrderHeader h WITH (NOLOCK)
            LEFT JOIN Purchase.MiscMaster mStatus WITH (NOLOCK) ON mStatus.Id = h.StatusId
            WHERE h.IsDeleted = 0
              AND h.UnitId = @UnitId
              AND h.PONumber LIKE @t
              AND (@poMethodId IS NULL OR h.POMethodId = @poMethodId)
              AND (@BudgetGroupId IS NULL OR h.BudgetGroupId = @BudgetGroupId)
              AND (mStatus.Code IS NULL OR mStatus.Code NOT IN (@Cancelled, @ForeClosed))
            ORDER BY h.Id DESC;";
        return _conn.QueryAsync<AutocompleteDto>(sql, new
        {
            t = $"%{term?.Trim()}%",
            poMethodId,
            UnitId = _ip.GetUnitId() ?? 0,
            budgetGroupId,
            Cancelled = MiscEnumEntity.Cancelled,
            ForeClosed = MiscEnumEntity.ForeClosed
        });
    }


   public async Task<(List<GetPOLocalPendingGroupDto> Rows, int Total)> GetPOPendingAsync(
    int? page, int? size, string? search, int? poId,int? poMethodId, CancellationToken ct)
    {
        var p = (page.HasValue && page > 0) ? page.Value : 1;
        var s = (size.HasValue && size > 0) ? size.Value : 15;
        var off = (p - 1) * s;
        var like = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
        var unitId = _ip.GetUnitId() ?? 0;

        // Only “Pending”
        var pending = await _miscMasterQueryRepository.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

        var sql = @"
            /* 1) Matching PO ids (both Local & Import exist) */
            CREATE TABLE #filtered (Id INT PRIMARY KEY);

            INSERT INTO #filtered (Id)
            SELECT DISTINCT h.Id
            FROM Purchase.PurchaseOrderHeader h
            LEFT JOIN Purchase.PurchaseLocalHeader  lh ON lh.PurchaseOrderId  = h.Id AND lh.IsDeleted = 0
            LEFT JOIN Purchase.PurchaseOrderImportHeader ih ON ih.PurchaseOrderId = h.Id AND ih.IsDeleted = 0
            WHERE h.IsDeleted = 0
            AND (@UnitId IS NULL OR h.UnitId = @UnitId)
            AND h.StatusId = @StatusId
            AND (@PoId IS NULL OR h.Id = @PoId)
            AND (@PoMethodId IS NULL OR h.PoMethodId = @PoMethodId)
            AND (@LikeSearch IS NULL OR h.PONumber LIKE @LikeSearch)
            AND (lh.Id IS NOT NULL OR ih.Id IS NOT NULL); -- ensure it’s either Local or Import

            /* 2) Page ids */
            CREATE TABLE #paged (Id INT PRIMARY KEY);
            WITH numbered AS (
                SELECT Id, ROW_NUMBER() OVER (ORDER BY Id DESC) rn
                FROM #filtered
            )
            INSERT INTO #paged (Id)
            SELECT Id FROM numbered WHERE rn BETWEEN (@off + 1) AND (@off + @size);

            /* 3) Header groups (common fields) */
            SELECT
                h.Id               ,
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
                mth.Code           AS POMethodCode,mth.Id as POMethodId
            FROM Purchase.PurchaseOrderHeader h
            LEFT JOIN Purchase.MiscMaster st  ON st.Id  = h.StatusId
            LEFT JOIN Purchase.MiscMaster cat ON cat.Id = h.POCategoryId
            LEFT JOIN Purchase.MiscMaster mth ON mth.Id = h.POMethodId
            WHERE h.Id IN (SELECT Id FROM #paged)
            ORDER BY h.Id DESC;

            /* 4) Unified Details:
                - Local: PurchaseLocalHeader/Detail
                - Import: PurchaseOrderImportHeader/Detail
            Map to the same DTO columns. Fields that don't exist in Import are NULL. */
            SELECT *
            FROM (
                /* Local detail */
                SELECT
                    d.Id,
                    d.PurchaseLocalId                       AS PurchaseLocalId,   -- parent id (local header)
                    d.IndentId,
                    d.ItemId,
                    d.ItemSno,
                    d.Quantity,
                    d.UnitPrice,
                    d.LastPOPrice,
                    d.DiscountTypeId,
                    d.DiscountValue,
                    d.PandFType,
                    d.PandFCharge,
                    d.OtherCharge,
                    d.GSTPercentage,d.CGSTPercentage,d.SGSTPercentage,d.IGSTPercentage,
                    d.CGST,
                    d.SGST,
                    d.IGST,
                    d.ScheduleDate,
                    d.DepartmentId,
                    d.ItemValue,
                    CAST(0 AS INT)              AS Edit,
                    CAST(NULL AS NVARCHAR(250)) AS EditReason
                FROM Purchase.PurchaseLocalDetail d
                JOIN Purchase.PurchaseLocalHeader lh
                ON lh.Id = d.PurchaseLocalId AND lh.IsDeleted = 0
                WHERE d.IsDeleted = 0
                AND lh.PurchaseOrderId IN (SELECT Id FROM #paged)

                UNION ALL

                /* Import detail -> project into the same columns */
                SELECT
                    ipd.Id,
                    ipd.PurchaseHeaderId                     AS PurchaseLocalId,   -- reuse alias: parent id (import header)
                    ipd.IndentId,
                    ipd.ItemId,
                    CAST(NULL AS INT)                        AS ItemSno,
                    ipd.Quantity,
                    ipd.UnitPrice,
                    CAST(NULL AS DECIMAL(18,6))              AS LastPOPrice,
                    CAST(NULL AS INT)                        AS DiscountTypeId,
                    CAST(NULL AS DECIMAL(18,6))              AS DiscountValue,
                    CAST(NULL AS INT)                        AS PandFType,
                    CAST(NULL AS DECIMAL(18,6))              AS PandFCharge,
                    ipd.OtherCharges                         AS OtherCharge,
                    CAST(NULL AS DECIMAL(18,6))              AS GSTPercentage,
                    CAST(NULL AS DECIMAL(18,6))              AS CGSTPercentage,
                    CAST(NULL AS DECIMAL(18,6))              AS SGSTPercentage,
                    CAST(NULL AS DECIMAL(18,6))              AS IGSTPercentage,
                    CAST(NULL AS DECIMAL(18,6))              AS CGST,
                    CAST(NULL AS DECIMAL(18,6))              AS SGST,
                    ipd.IGST                                  AS IGST,
                    CAST(NULL AS DATETIMEOFFSET)             AS ScheduleDate,
                    CAST(NULL AS INT)                        AS DepartmentId,
                    ipd.TotalValue                           AS ItemValue,
                    CAST(0 AS INT)                           AS Edit,
                    CAST(NULL AS NVARCHAR(250))              AS EditReason
                FROM Purchase.PurchaseOrderImportDetail ipd
                JOIN Purchase.PurchaseOrderImportHeader iph
                ON iph.Id = ipd.PurchaseHeaderId AND iph.IsDeleted = 0
                WHERE iph.PurchaseOrderId IN (SELECT Id FROM #paged)
            ) u
            ORDER BY PurchaseLocalId, Id;

            /* 4b) Map parent detail id -> PurchaseOrderId for both Local and Import */
            SELECT*
            FROM (
                SELECT lh.Id AS PurchaseLocalId, lh.PurchaseOrderId
                FROM Purchase.PurchaseLocalHeader lh
                WHERE lh.IsDeleted = 0
                AND lh.PurchaseOrderId IN (SELECT Id FROM #paged)

                UNION ALL

                SELECT iph.Id AS PurchaseLocalId, iph.PurchaseOrderId
                FROM Purchase.PurchaseOrderImportHeader iph
                WHERE iph.IsDeleted = 0
                AND iph.PurchaseOrderId IN (SELECT Id FROM #paged)
            ) m;

            /* 5) Total groups */
            SELECT COUNT(1) FROM #filtered;

            /* 6) Cleanup */
            DROP TABLE #paged;
            DROP TABLE #filtered;";

        var param = new
        {
            UnitId = unitId,
            StatusId = pending.Id,
            PoId = poId,
            PoMethodId = poMethodId,
            LikeSearch = like,
            off,
            size = s
        };

        using var multi = await _conn.QueryMultipleAsync(
            new CommandDefinition(sql, param, cancellationToken: ct));

        // 3) headers
        var headers = (await multi.ReadAsync<GetPOLocalPendingGroupDto>()).ToList();

        // 4) details (unified)
        var details = (await multi.ReadAsync<PurchaseLocalDetailDto>()).ToList();

        // 4b) parentId -> POId map (works for both local & import; parentId = PurchaseLocalId alias)
        var mapRows = (await multi.ReadAsync<(int PurchaseLocalId, int PurchaseOrderId)>()).ToList();
        var poByParentId = mapRows
            .GroupBy(x => x.PurchaseLocalId)
            .ToDictionary(g => g.Key, g => g.First().PurchaseOrderId);

        // 5) total
        var total = await multi.ReadFirstAsync<int>();

        // attach
        var byPo = headers.ToDictionary(h => h.Id, h => h);
        foreach (var g in headers) g.Lines ??= new List<GetPOLocalPendingDto>();

        foreach (var d in details)
        {
            if (!poByParentId.TryGetValue(d.PurchaseLocalId, out var poIdForDetail)) continue;
            if (!byPo.TryGetValue(poIdForDetail, out var grp)) continue;

            grp.Lines.Add(new GetPOLocalPendingDto
            {
                Id = d.Id,
                PurchaseLocalId = d.PurchaseLocalId, // parent id: local header id or import header id
                IndentId = d.IndentId,
                ItemId = d.ItemId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                LastPOPrice = d.LastPOPrice,
                DiscountTypeId = d.DiscountTypeId ?? 0,
                DiscountValue = d.DiscountValue,
                PandFType = d.PandFType,
                PandFCharge = d.PandFCharge,
                OtherCharge = d.OtherCharge,
                GSTPercentage = d.GSTPercentage,                
                CGST = d.CGST,
                SGST = d.SGST,
                IGST = d.IGST,
                CGSTPercentage  = (d.IGST > 0) ? d.GSTPercentage / 2 : 0,
                SGSTPercentage  = (d.IGST > 0) ? d.GSTPercentage / 2 : 0,
                IGSTPercentage  = (d.IGST > 0) ? 0 : d.GSTPercentage,
                ScheduleDate = d.ScheduleDate,
                DepartmentId = d.DepartmentId,
                ItemValue = d.ItemValue
            });
        }

        return (headers, total);
    }

    public async Task<bool> HasAnyGrnAsync(int poId, CancellationToken ct)
    {
        const string sql = @"
                SELECT TOP 1 1
                FROM Purchase.GRNDetail g
                WHERE g.PoId = @poId;";

        var exists = await _conn.ExecuteScalarAsync<int?>(sql, new { poId });
        return exists.HasValue;
    }

    public async Task<string?> GetStatusCodeAsync(int poId, CancellationToken ct)
    {
        const string sql = @"
                SELECT m.Code
                FROM Purchase.PurchaseOrderHeader h
                LEFT JOIN Purchase.MiscMaster m ON m.Id = h.StatusId
                WHERE h.Id = @poId AND h.IsDeleted = 0;";

        return await _conn.ExecuteScalarAsync<string?>(sql, new { poId });
    }
    public async Task<bool> ExistsAsync(int poId, CancellationToken ct)
    {
        const string sql = @"
                SELECT TOP 1 1
                FROM Purchase.PurchaseOrderHeader h
                WHERE h.Id = @Id
                AND h.IsDeleted = 0
                AND h.UnitId = @UnitId;";

        var val = await _conn.ExecuteScalarAsync<int?>(
            new CommandDefinition(
                sql,
                new { Id = poId, UnitId = _ip.GetUnitId() ?? 0 },
                cancellationToken: ct));

        return val.HasValue;
    }
    public async Task<int> GetNextRevisionAsync(int rootPoId, CancellationToken ct)
    {
        // rootPoId = first PO in the chain (OldPOId=null). If a mid-po is passed, normalize.
        var root = await _applicationDb.PurchaseOrderHeaders
            .Where(p => p.Id == rootPoId)
            .Select(p => p.OldPOId == null ? p.Id : p.OldPOId.Value)
            .FirstOrDefaultAsync(ct);

        if (root == 0) return 1;

        var maxRev = await _applicationDb.PurchaseOrderHeaders
            .Where(p => p.Id == root || p.OldPOId == root)
            .MaxAsync(p => (int?)p.RevisionNo, ct) ?? 0;

        return maxRev + 1;
    }
    public async Task<List<LastPoPriceDto>> LastPOPriceByItemIdAsync(List<int> itemIds)
    {
          var approved = await _miscMasterQueryRepository.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);

        const string sql = @"
           ;WITH P AS (
          SELECT
              PLD.ItemId,
              POH.PODate,
              PLD.UnitPrice,
              ROW_NUMBER() OVER (
                PARTITION BY PLD.ItemId
                ORDER BY POH.PODate DESC, POH.Id DESC, PLD.Id DESC
              ) AS rn
          FROM Purchase.PurchaseOrderHeader  AS POH
          JOIN Purchase.PurchaseLocalHeader  AS PLH ON PLH.PurchaseOrderId  = POH.Id
          JOIN Purchase.PurchaseLocalDetail  AS PLD ON PLD.PurchaseLocalId  = PLH.Id
          WHERE PLD.ItemId IN @ItemIds and POH.statusId = @Approved
        )
        SELECT ItemId,
               UnitPrice AS LastPOPrice,
               PODate    AS LastPODate
        FROM P
        WHERE rn = 1
        ORDER BY ItemId;
            ";
        var result = await _conn.QueryAsync<LastPoPriceDto>(sql, new { ItemIds = itemIds, Approved=approved.Id });
        return result.ToList();
    }

    public async Task<bool> SoftDeleteValidationAsync(int id)
    {
        // GateEntryDetail and GrnDetail do NOT have IsDeleted/IsActive columns
        // PurchaseDocuments, ImportPOHeader, ServiceHeader have IsDeleted
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Purchase].[GateEntryDetail] WHERE PoId = @id)
                OR
                EXISTS (SELECT 1 FROM [Purchase].[GrnDetail] WHERE PoId = @id)
                OR
                EXISTS (SELECT 1 FROM [Purchase].[PurchaseDocuments] WHERE PoId = @id)
                OR
                EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderImportHeader] WHERE PurchaseOrderId = @id AND IsDeleted = 0)
                OR
                EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderServiceHeader] WHERE PurchaseOrderId = @id AND IsDeleted = 0)
            THEN 1 ELSE 0 END;";

        return await _conn.ExecuteScalarAsync<bool>(sql, new { id });
    }

    public async Task<decimal> GetTotalPurchaseValueAsync(
        int? budgetGroupId, int? itemCategoryId, int? poMethodId,
        DateTimeOffset poDate,
        CancellationToken ct)
    {
        const string sql = @"
            SELECT ISNULL(SUM(h.PurchaseValue), 0)
            FROM Purchase.PurchaseOrderHeader h WITH (NOLOCK)
            inner join Purchase.MiscMaster m WITH (NOLOCK) on m.Id = h.POCategoryId
            WHERE h.IsDeleted = 0
              AND h.UnitId = @UnitId
              AND h.StatusId = (
                    SELECT TOP 1 m.Id
                    FROM Purchase.MiscMaster m WITH (NOLOCK)
                    inner join Purchase.MiscTypeMaster mt WITH (NOLOCK) on mt.Id = m.MiscTypeId
                    WHERE m.Code = @ApprovedCode and mt.MiscTypeCode =  @Status AND m.IsDeleted = 0
              )
              AND (@BudgetGroupId IS NULL OR h.BudgetGroupId = @BudgetGroupId)
              AND (@ItemCategoryId IS NULL OR h.ItemCategoryId = @ItemCategoryId)
              AND (@PoMethodId IS NULL OR h.POMethodId = @PoMethodId)
              and m.Code = @POCategoryId
              AND MONTH(h.PODate) = @Month
              AND YEAR(h.PODate)  = @Year;";

        return await _conn.ExecuteScalarAsync<decimal>(
            new CommandDefinition(sql, new
            {
                UnitId = _ip.GetUnitId() ?? 0,
                ApprovedCode = MiscEnumEntity.Approved,
                BudgetGroupId = budgetGroupId,
                ItemCategoryId = itemCategoryId,
                PoMethodId = poMethodId,
                Month = poDate.Month,
                Year = poDate.Year,
                Status = MiscEnumEntity.ApprovalStatus,
                POCategoryId = MiscEnumEntity.EmergencyPO
            }, cancellationToken: ct));
    }

    public async Task<bool> NotFoundAsync(int id, CancellationToken ct)
    {
        const string sql = @"
            SELECT CASE WHEN NOT EXISTS (
                SELECT 1 FROM Purchase.PurchaseOrderHeader
                WHERE Id = @Id AND IsDeleted = 0
            ) THEN 1 ELSE 0 END";

        return await _conn.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }
}
