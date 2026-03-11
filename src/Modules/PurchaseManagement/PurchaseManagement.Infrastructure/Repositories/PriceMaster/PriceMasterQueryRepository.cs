#nullable disable
using System.Data;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Application.PriceMaster.Queries.GetPriceMasterPending;
using PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PriceMaster;
using Dapper;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.PriceMaster
{
    public class PriceMasterQueryRepository(
        ApplicationDbContext db,
      IIPAddressService _ipAddress, IDbConnection _db,IMiscMasterQueryRepository _miscMasterQueryRepository
    ) : IPriceMasterQueryRepository
    {
        public async Task<PriceMasterUpdateDto> GetForEditAsync(int id, CancellationToken ct)
        {
            const string headerSql = @"
                SELECT Id, ItemId, VendorId, CurrencyCode, ValidFrom, ValidTo,
                    IsApprove, SourceFromId, SourceDetailId
                FROM Purchase.PriceMasterHeader WITH (NOLOCK)
                WHERE Id = @Id AND IsDeleted = 0;";

            const string detailSql = @"
                SELECT Id, PriceMasterHeaderId, ScaleQtyFrom, ScaleQtyTo, UnitPrice, UomId
                FROM Purchase.PriceMasterDetails WITH (NOLOCK)
                WHERE PriceMasterHeaderId = @Id AND IsDeleted = 0
                ORDER BY ScaleQtyFrom, COALESCE(ScaleQtyTo, 999999999);";

            using var multi = await _db.QueryMultipleAsync($"{headerSql}{detailSql}", new { Id = id });
            var h = await multi.ReadFirstOrDefaultAsync<PriceMasterUpdateDto>();
            if (h is null) return null;

            h.Details.AddRange((await multi.ReadAsync<PriceMasterDetailUpsertDto>()).ToList());
            return h;
        }
        public async Task<PriceMasterGetAllDto> GetByIdAsync(int id, CancellationToken ct)
        {
            var unitId = _ipAddress.GetUnitId() ?? 0;

            var h = await db.Set<PriceMasterHeader>()
                .AsNoTracking()
                .Include(x => x.Details.Where(d => d.IsDeleted == 0))
                .Include(x => x.MiscStatus) // <-- so StatusName is available
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsDeleted == 0 &&
                    x.UnitId == unitId &&
                    x.IsActive == PurchaseManagement.Domain.Common.BaseEntity.Status.Active, ct);

            if (h is null) return null;
            var sourceFromCode = await db.Set<PurchaseManagement.Domain.Entities.MiscMaster>()
                .AsNoTracking()
                .Where(m => m.Id == h.SourceFromId && m.IsDeleted == 0)
                .Select(m => m.Code)
                .FirstOrDefaultAsync(ct);

            var details = h.Details
                .OrderBy(d => d.ScaleQtyFrom)
                .ThenBy(d => d.ScaleQtyTo ?? decimal.MaxValue)
                .Select(d => new PriceMasterDetailUpsertDto
                {
                    Id           = d.Id,
                    ScaleQtyFrom = d.ScaleQtyFrom,
                    ScaleQtyTo   = d.ScaleQtyTo,
                    UnitPrice    = d.UnitPrice,
                    CurrencyId   = d.CurrencyId,
                    IsActive     = (int)d.IsActive   
                })
                .ToList();

            return new PriceMasterGetAllDto
            {
                Id             = h.Id,
                ItemId         = h.ItemId,
                ItemCode       = null,
                ItemName       = null,
                VendorId       = h.VendorId,
                VendorCode     = null,
                VendorName     = null,
                ValidFrom      = h.ValidFrom,
                ValidTo        = h.ValidTo,
                StatusId       = h.StatusId,
                StatusName     = h.MiscStatus?.Code, // or = statusCode if you used the query above
                SourceFromId   = h.SourceFromId,
                SourceFrom     = sourceFromCode,
                SourceDetailId = h.SourceDetailId ?? 0,
                UomId          = h.UomId,
                UOM            = null,
                Details        = details
            };
        }


        public async Task<PurchaseManagement.Application.Common.PagedResult<PriceMasterGetAllDto>> GetAllAsync(
            int? page, 
            int? size, 
            string searchTerm, 
            int? itemId, 
            decimal? qtyFrom, 
            decimal? qtyTo, int? statusId,bool expiredList,
            CancellationToken ct)
        {
            var unitId = _ipAddress.GetUnitId() ?? 0;
            var includeExpired = expiredList == true;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var q = db.Set<PriceMasterHeader>()
                .AsNoTracking()
                .Where(h => h.IsDeleted == 0 && h.UnitId == unitId);
          
            if (itemId.HasValue && itemId.Value > 0)
                q = q.Where(h => h.ItemId == itemId.Value);
            
             if (statusId.HasValue && statusId.Value > 0)
                q = q.Where(h => h.StatusId == statusId.Value); 
            
            if (!expiredList)
            {
                q = q.Where(h =>
                    h.IsActive == BaseEntity.Status.Active &&                    
                    (h.ValidTo == null || h.ValidTo >= today)
                );
            }
            
            if (expiredList)
            {
                q = q.Where(h =>                    
                    h.ValidTo != null && h.ValidTo < today
                );
            }

            if (qtyFrom.HasValue || qtyTo.HasValue)
            {
                var from = qtyFrom;
                var to = qtyTo;

                q = q.Where(h => db.Set<PriceMasterDetail>().Any(d =>
                    d.PriceMasterHeaderId == h.Id &&
                    d.IsDeleted == 0 &&
                    (
                        (from.HasValue && !to.HasValue &&
                            d.ScaleQtyFrom <= from.Value &&
                            (d.ScaleQtyTo == null || from.Value <= d.ScaleQtyTo))
                    || (!from.HasValue && to.HasValue &&
                            d.ScaleQtyFrom <= to.Value &&
                            (d.ScaleQtyTo == null || to.Value <= d.ScaleQtyTo))
                    || (from.HasValue && to.HasValue &&
                            d.ScaleQtyFrom <= to.Value &&
                            (d.ScaleQtyTo ?? decimal.MaxValue) >= from.Value)
                    )));
            }

            var total = await q.CountAsync(ct);

            var p = page ?? 1;
            var s = size ?? 15;

            var headers = await q
                .OrderByDescending(h => h.Id)
                .Skip((p - 1) * s)
                .Take(s)
                .Select(h => new PriceMasterGetAllDto
                {
                    Id = h.Id,
                    ItemId = h.ItemId,
                    VendorId = h.VendorId,
                    ValidFrom = h.ValidFrom,
                    ValidTo = h.ValidTo,
                    StatusId = h.StatusId,
                    StatusName = h.MiscStatus.Code,
                    SourceFromId = h.SourceFromId,
                    SourceFrom = h.MiscSourceFrom.Code,
                    SourceDetailId = h.SourceDetailId ?? 0,
                    UomId = h.UomId,
                    IsActive = (int)h.IsActive,
                    Details = new List<PriceMasterDetailUpsertDto>()
                })
                .ToListAsync(ct);

            return new PurchaseManagement.Application.Common.PagedResult<PriceMasterGetAllDto>
            {
                Items = headers,
                Total = total,
                Page = p,
                PageSize = s
            };
        }

        public async Task<(List<PriceMasterPendingGroupDto>, int)> GetPriceMasterPendingAsync(
            int PageNumber, int PageSize, string SearchTerm)
        {
            var unitId = _ipAddress.GetUnitId() ?? 0;
            var page   = Math.Max(1, PageNumber);
            var size   = Math.Max(1, PageSize);
            var offset = (page - 1) * size;
            var status = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.ApprovalStatus,
                MiscEnumEntity.Pending);

            var sql = @"
            IF OBJECT_ID('tempdb..#filtered') IS NOT NULL DROP TABLE #filtered;
            IF OBJECT_ID('tempdb..#groups')   IS NOT NULL DROP TABLE #groups;
            IF OBJECT_ID('tempdb..#pg')       IS NOT NULL DROP TABLE #pg;

            ;WITH base AS (
                SELECT
                    PH.Id,
                    PH.ItemId,
                    PH.VendorId,
                    PH.ValidFrom,
                    PH.ValidTo,
                    PH.UomId,
                    PH.CreatedByName  AS CreatedBy,
                    PH.CreatedDate    AS CreatedDate,

                    PD.ScaleQtyFrom,
                    PD.ScaleQtyTo,
                    PD.UnitPrice,
                    PD.CurrencyId
                FROM Purchase.PriceMasterHeader  AS PH
                JOIN Purchase.PriceMasterDetail  AS PD ON PD.PriceMasterHeaderId = PH.Id               
                -- SourceFrom = Direct (adjust if you need other codes)
                JOIN Purchase.MiscTypeMaster     AS MTf ON MTf.MiscTypeCode  = 'SourceFrom'
                JOIN Purchase.MiscMaster         AS MF  ON MF.MiscTypeId     = MTf.Id AND MF.Id = PH.SourceFromId AND MF.Code = @sourceFrom
                WHERE PH.UnitId = @UnitId and PH.StatusId = @Status
                AND PH.IsDeleted = 0
                AND PH.IsActive  = 1
                AND PD.IsDeleted = 0
                AND PD.IsActive  = 1
            ),
            filtered AS (
                SELECT * FROM base
                WHERE (@Search IS NULL
                    OR CAST(ItemId   AS nvarchar(50)) LIKE '%' + @Search + '%'
                    OR CAST(VendorId AS nvarchar(50)) LIKE '%' + @Search + '%'
                    OR CONVERT(nvarchar(30), ValidFrom, 23) LIKE '%' + @Search + '%'
                    OR CONVERT(nvarchar(30), ValidTo,   23) LIKE '%' + @Search + '%')
            )
            -- keep all rows (details) for the paged groups
            SELECT
                Id, ItemId, VendorId, ValidFrom, ValidTo, UomId, CreatedBy, CreatedDate,
                ScaleQtyFrom, ScaleQtyTo, UnitPrice, CurrencyId
            INTO #filtered
            FROM filtered;

            -- distinct groups for correct total & paging
            SELECT DISTINCT
                Id, ItemId, VendorId, ValidFrom, ValidTo, UomId, CreatedBy, CreatedDate
            INTO #groups
            FROM #filtered;

            -- total = number of header groups (not detail rows)
            SELECT COUNT(1) FROM #groups;

            ;WITH g AS (
                SELECT *,
                    ROW_NUMBER() OVER (ORDER BY ValidFrom DESC, Id ASC) AS rn
                FROM #groups
            )
            SELECT
                Id, ItemId, VendorId, ValidFrom, ValidTo, UomId, CreatedBy, CreatedDate
            INTO #pg
            FROM g
            WHERE rn BETWEEN @Offset + 1 AND @Offset + @PageSize;

            -- return details for the paged groups only
            SELECT
                f.Id, f.ItemId, f.VendorId, f.ValidFrom, f.ValidTo, f.UomId, p.CreatedBy, p.CreatedDate,
                f.ScaleQtyFrom, f.ScaleQtyTo, f.UnitPrice, f.CurrencyId
            FROM #filtered f
            JOIN #pg p
            ON p.Id      = f.Id
            AND p.ItemId  = f.ItemId
            AND p.VendorId= f.VendorId
            AND p.ValidFrom = f.ValidFrom
            AND ( (p.ValidTo IS NULL AND f.ValidTo IS NULL) OR p.ValidTo = f.ValidTo )
            AND p.UomId   = f.UomId
            ORDER BY p.ValidFrom DESC, p.Id ASC, f.ScaleQtyFrom ASC;

            DROP TABLE #filtered;
            DROP TABLE #groups;
            DROP TABLE #pg;";

                var args = new
                {
                    UnitId = unitId,
                    Search = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                    Offset = offset,
                    PageSize = size,
                    Status = status.Id,
                    sourceFrom = MiscEnumEntity.SourceFromDirect
                };

                using var multi = await _db.QueryMultipleAsync(sql, args);
                var total    = await multi.ReadSingleAsync<int>();
                var flatRows = (await multi.ReadAsync<PriceMasterPendingRowDto>()).ToList();

                // group headers + lines
                var items = flatRows
                    .GroupBy(r => new { r.Id, r.ItemId, r.VendorId, r.ValidFrom, r.ValidTo, r.UomId, r.CreatedBy, r.CreatedDate })
                    .Select(g =>
                    {
                        var head = g.Key;
                        return new PriceMasterPendingGroupDto
                        {
                            Id        = head.Id,
                            ItemId    = head.ItemId,
                            VendorId  = head.VendorId,
                            ValidFrom = head.ValidFrom,
                            ValidTo   = head.ValidTo,
                            UomId     = head.UomId,                            
                            CreatedByName   = head.CreatedBy ?? string.Empty,
                            CreatedDate = head.CreatedDate,                            
                            SupplierName = string.Empty,
                            ItemName     = string.Empty,
                            UOM          = string.Empty,

                            Lines = g.Select(r => new PriceMasterPendingDto
                            {
                                QuantityFrom = r.ScaleQtyFrom,
                                QuantityTo   = r.ScaleQtyTo ?? 0, 
                                UnitPrice    = r.UnitPrice,
                                CurrencyId   = r.CurrencyId,
                                CurrencyName = string.Empty       
                            }).ToList()
                        };
                    })
                    .ToList();

                return (items, total);
            }
        public async Task<List<UnitPriceDto>> GetUnitPriceByQtyANDItemId(IEnumerable<ItemQtyDto> items)
        {

            var list = items?.ToList() ?? new List<ItemQtyDto>();
            if (list.Count == 0) return new List<UnitPriceDto>();

            var unitId = _ipAddress.GetUnitId() ?? 0;
            const string statusCode = MiscEnumEntity.Approved;

            // Build TVP
            var tvp = new DataTable();
            tvp.Columns.Add("ItemId", typeof(int));
            tvp.Columns.Add("Qty", typeof(decimal));
            foreach (var x in list)
                tvp.Rows.Add(x.ItemId, x.Qty);

            // Most-recent valid header per item + best matching qty tier
            const string sql = @"
       
        SELECT x.ItemId,
               pick.HeaderId           AS PriceMasterHeaderId,
               pick.UnitPrice
        FROM @ItemQtys AS x
        CROSS APPLY (
            SELECT TOP (1)
                   pmh.Id,
                   pmd.UnitPrice,
                   pmh.ValidFrom,
                   pmd.ScaleQtyFrom
            FROM Purchase.PriceMasterHeader pmh
            JOIN Purchase.MiscMaster      mm  ON mm.Id = pmh.StatusId
            JOIN Purchase.PriceMasterDetail pmd ON pmd.PriceMasterHeaderId = pmh.Id
            WHERE pmh.UnitId = @UnitId
              AND mm.Code   = @Status
             
              AND CAST(GETDATE() AS date) BETWEEN pmh.ValidFrom AND ISNULL(pmh.ValidTo, '9999-12-31')
              AND pmh.ItemId = x.ItemId
              AND (pmd.ScaleQtyFrom IS NULL OR pmd.ScaleQtyFrom <= x.Qty)
              AND (pmd.ScaleQtyTo   IS NULL OR x.Qty <= pmd.ScaleQtyTo)
            ORDER BY pmh.ValidFrom DESC,      
                     ISNULL(pmd.ScaleQtyFrom,0) DESC, 
                     pmd.Id DESC
        ) AS pick (HeaderId, UnitPrice, ValidFrom, ScaleQtyFrom)
        ORDER BY x.ItemId;";

            var p = new DynamicParameters();
            p.Add("@UnitId", unitId);
            p.Add("@Status", statusCode);
            p.Add("@ItemQtys", tvp.AsTableValuedParameter("dbo.ItemQtyList"));
            try
            {
                var rows = await _db.QueryAsync<UnitPriceDto>(sql, p);
                return rows.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<UnitPriceDto>();
            }
            
           
    }

    }
}
