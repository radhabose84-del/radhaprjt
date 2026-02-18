#nullable disable
using System.Data;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparison;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonById;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonPending;
using PurchaseManagement.Domain.Common;
using Dapper;
using static PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparison.QuoteComparisonDto;

namespace PurchaseManagement.Infrastructure.Repositories.Quotation.QuotationCompare
{
    public class QuotationCompareQueryRepository : IQuotationCompareQueryRepository
    {
    private readonly IDbConnection _dbConnection;
    private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
    private readonly IIPAddressService _ip;
        public QuotationCompareQueryRepository(IDbConnection dbConnection, IIPAddressService ip, IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _dbConnection = dbConnection;
            _ip = ip;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }

        public async Task<QuoteCompareByIdDto> GetByIdQuoteCompareAsync(int id, CancellationToken ct = default)
        {
            var sql = "SELECT Id,RfqId,RfqCode,StatusId FROM Purchase.QuotationComparisonHeader WHERE Id = @Id ";

            return await _dbConnection.QueryFirstOrDefaultAsync<QuoteCompareByIdDto>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));    
        }

        public async Task<QuoteComparisonDto> GetQuoteComparisonAsync(int rfqId, CancellationToken cancellationToken)
    {
        var approved = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.ApprovalStatus,
                MiscEnumEntity.Approved);

        var sql = @"
            SELECT 
            RM.Id AS RfqId,
            RM.RfqCode,
            RI.ItemId,
            RI.Quantity,
            RI.UomId,
            RS.SupplierId,
            RS.Name AS SupplierName,
            QD.Rate AS UnitPrice,
            ISNULL(QH.Freight,0) as Freight,
            CAST(ISNULL(QH.Freight,0) / NULLIF(ItemCount.ItemCnt,0) AS DECIMAL(18,2)) AS Freight_PerItem,
            ISNULL(QD.Discount,0) AS DiscountValue,
            ISNULL(QD.GstPercent,0) AS GstPercent,
            ISNULL(QD.GstAmount,0) AS GstAmount,
            QH.ValidTill,
            QD.DeliveryDays,
            (QD.Rate - ISNULL(QD.Discount,0)) AS Net,
            (QD.Rate - ISNULL(QD.Discount,0)) + ISNULL(QD.GstAmount,0) + (ISNULL(QH.Freight,0) / NULLIF(ItemCount.ItemCnt,0)) AS LandedUnit,
            RI.Quantity * ((QD.Rate - ISNULL(QD.Discount,0)) + ISNULL(QD.GstAmount,0)) 
                + (ISNULL(QH.Freight,0) / NULLIF(ItemCount.ItemCnt,0)) AS Total,
                QH.Id as QuotationHeaderId,
                QD.Id as QuotationDetailId,
                CASE 
        WHEN EXISTS (
                    SELECT 1 
                    FROM Purchase.QuotationComparisonHeader QCH 
                    WHERE QCH.RfqId = RM.Id AND QCH.StatusId = @Approved
                ) THEN 1 ELSE 0
                  END AS IsPosted
        FROM Purchase.RfqMaster RM
        INNER JOIN Purchase.RfqItem RI 
            ON RM.Id = RI.RfqId
        INNER JOIN Purchase.QuotationDetail QD 
            ON QD.ItemId = RI.ItemId
        INNER JOIN Purchase.QuotationHeader QH 
            ON QH.Id = QD.QuotationHeaderId AND QH.RfqId = RM.Id
        INNER JOIN Purchase.RfqSuppliers RS 
            ON RS.RfqId = RM.Id AND RS.SupplierId = QH.SupplierId
        CROSS APPLY (
            SELECT COUNT(*) AS ItemCnt
            FROM Purchase.RfqItem RI2
            WHERE RI2.RfqId = RM.Id
        ) AS ItemCount
        WHERE RM.Id = @RfqId
        ORDER BY RI.Id;
        ";

        var rows = await _dbConnection.QueryAsync<QuoteComparisonRow>(sql, new { RfqId = rfqId,Approved =  approved.Id });

        QuoteComparisonDto result = null;
        var lookup = new Dictionary<int, QuoteItemDto>();

        foreach (var row in rows)
        {
            if (result == null)
            {
                result = new QuoteComparisonDto
                {
                    RfqId = row.RfqId,
                    RfqCode = row.RfqCode,
                    IsPosted = row.IsPosted                    
                };
            }

            if (!lookup.TryGetValue(row.ItemId, out var itemDto))
            {
                itemDto = new QuoteItemDto
                {
                    ItemId = row.ItemId,
                    Quantity = row.Quantity,
                    UomId = row.UomId   // ✅ Fix: map UomId
                };
                lookup[row.ItemId] = itemDto;
                result.Items.Add(itemDto);
            }

            itemDto.Suppliers.Add(new QuoteSupplierDto
            {
                QuotationHeaderId = row.QuotationHeaderId,
                QuotationDetailId = row.QuotationDetailId,
                SupplierId = row.SupplierId,
                SupplierName = row.SupplierName,
                UnitPrice = row.UnitPrice,
                DiscountValue = row.DiscountValue,
                Freight = row.Freight,
                Freight_PerItem = row.Freight_PerItem,
                GstPercent = row.GstPercent,
                GstAmount = row.GstAmount,
                ValidTill = row.ValidTill,
                DeliveryDays = row.DeliveryDays,
                Net = row.Net,
                LandedUnit = row.LandedUnit,
                Total = row.Total
                
            });
        }
        return result;
    }
      public async Task<(List<QuoteComparisonPendingGroupDto>, int)> GetQuoteComparisonPendingAsync(
            int PageNumber, int PageSize, string SearchTerm)
        {
            var unitId = _ip.GetUnitId();
            var page    = Math.Max(1, PageNumber);
            var size    = Math.Max(1, PageSize);
            var offset  = (page - 1) * size;

            var sql = @"
                IF OBJECT_ID('tempdb..#filtered') IS NOT NULL DROP TABLE #filtered;
                IF OBJECT_ID('tempdb..#groups')   IS NOT NULL DROP TABLE #groups;
                IF OBJECT_ID('tempdb..#pg')       IS NOT NULL DROP TABLE #pg;

                ;WITH base AS (
                    SELECT DISTINCT
                        QCH.Id ,
                        QCH.RfqId,
                        RM.RfqCode,

                        -- line-level columns
                        QD.ItemId,
                        QD.Quantity,
                        QD.UomId,
                        QD.HsnId,
                        QD.Rate,
                        QD.Discount,
                        QD.GstPercent,
                        QD.GstAmount,
                        QD.Total,
                        RS.SupplierId,
                        RS.Name AS SupplierName,

                        -- group-level audit fields
                        QCH.CreatedByName AS ComparisonCreatedBy,
                        QCH.CreatedDate   AS ComparisonCreatedDate,
                        QH.CreatedByName  AS QuotationCreatedBy,
                        QH.CreatedDate    AS QuotationCreatedDate,
                        RM.CreatedByName  AS RFQCreatedBy,
                        RM.CreatedDate    AS RFQCreatedDate
                    FROM Purchase.QuotationComparisonHeader   AS QCH
                    JOIN Purchase.QuotationComparisonDetail   AS QCD ON QCH.Id = QCD.QuotationComparisonHeaderId
                    JOIN Purchase.QuotationHeader             AS QH  ON QH.Id  = QCD.QuotationHeaderId
                    JOIN Purchase.QuotationDetail             AS QD  ON QD.QuotationHeaderId = QH.Id AND QCD.QuotationDetailId = QD.Id
                    JOIN Purchase.RfqMaster                   AS RM  ON RM.Id = QCH.RfqId
                    JOIN Purchase.RfqItem                     AS RI  ON RI.RfqId = RM.Id AND RI.ItemId = QD.ItemId
                    JOIN Purchase.RfqSuppliers                AS RS  ON RS.RfqId = RM.Id AND QH.SupplierId = RS.SupplierId
                    JOIN Purchase.MiscTypeMaster              AS MTy ON MTy.MiscTypeCode = 'ApprovalStatus'
                    JOIN Purchase.MiscMaster                  AS MSt ON MSt.MiscTypeId   = MTy.Id
                    WHERE RM.UnitId = @UnitId
                    AND MSt.Code  = 'Pending'
                    AND QH.IsDeleted = 0
                    AND QH.IsActive  = 1
                ),
                filtered AS (
                    SELECT * FROM base
                    WHERE (@Search IS NULL 
                        OR RfqCode LIKE '%' + @Search + '%'
                        OR CAST(ItemId     AS nvarchar(50)) LIKE '%' + @Search + '%'
                        OR CAST(SupplierId AS nvarchar(50)) LIKE '%' + @Search + '%')
                )
                -- Keep all rows (for lines)
                SELECT
                    Id, RfqId, RfqCode,
                    ItemId, Quantity, UomId, HsnId, Rate, Discount, GstPercent, GstAmount, Total,
                    SupplierId, SupplierName,
                    ComparisonCreatedBy, ComparisonCreatedDate,
                    QuotationCreatedBy,  QuotationCreatedDate,
                    RFQCreatedBy,        RFQCreatedDate
                INTO #filtered
                FROM filtered;

                -- Distinct groups for counting/paging
                SELECT DISTINCT
                    Id, RfqId, RfqCode,
                    ComparisonCreatedBy, ComparisonCreatedDate,
                    QuotationCreatedBy,  QuotationCreatedDate,
                    RFQCreatedBy,        RFQCreatedDate
                INTO #groups
                FROM #filtered;

                -- total number of groups
                SELECT COUNT(1) FROM #groups;

                -- page groups
                ;WITH g AS (
                    SELECT *,
                        ROW_NUMBER() OVER (ORDER BY RfqCode, Id) AS rn
                    FROM #groups
                )
                SELECT
                    Id, RfqId, RfqCode,
                    ComparisonCreatedBy, ComparisonCreatedDate,
                    QuotationCreatedBy,  QuotationCreatedDate,
                    RFQCreatedBy,        RFQCreatedDate
                INTO #pg
                FROM g
                WHERE rn BETWEEN @Offset + 1 AND @Offset + @PageSize;

                -- return line rows for the paged groups
                SELECT
                    f.Id, f.RfqId, f.RfqCode,
                    f.ItemId, f.Quantity, f.UomId, f.HsnId, f.Rate, f.Discount, f.GstPercent, f.GstAmount, f.Total,
                    f.SupplierId, f.SupplierName,
                    p.ComparisonCreatedBy, p.ComparisonCreatedDate,
                    p.QuotationCreatedBy,  p.QuotationCreatedDate,
                    p.RFQCreatedBy,        p.RFQCreatedDate
                FROM #filtered f
                JOIN #pg p
                ON p.Id = f.Id
                AND p.RfqId        = f.RfqId
                AND p.RfqCode      = f.RfqCode
                ORDER BY p.RfqCode, p.Id, f.ItemId, f.SupplierId;

                DROP TABLE #filtered;
                DROP TABLE #groups;
                DROP TABLE #pg;
                ";

            var args = new
            {
                UnitId = unitId,
                Search = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                Offset = offset,
                PageSize = size
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, args);
            var total = await multi.ReadSingleAsync<int>();

            // Read all row-level records (for the paged groups)
            var flatRows = (await multi.ReadAsync<QuoteComparisonPendingRowDto>()).ToList();

            // Group into headers with lines; copy audit fields from the first row of each group
            var items = flatRows
                .GroupBy(r => new { r.Id, r.RfqId, r.RfqCode })
                .Select(g =>
                {
                    var head = g.First();
                    return new QuoteComparisonPendingGroupDto
                    {
                        Id = g.Key.Id,
                        RfqId        = g.Key.RfqId,
                        RfqCode      = g.Key.RfqCode,

                        ComparisonCreatedBy   = head.ComparisonCreatedBy,
                        ComparisonCreatedDate = head.ComparisonCreatedDate,
                        QuotationCreatedBy    = head.QuotationCreatedBy,
                        QuotationCreatedDate  = head.QuotationCreatedDate,
                        RFQCreatedBy          = head.RFQCreatedBy,
                        RFQCreatedDate        = head.RFQCreatedDate,

                        Lines = g.Select(r => new QuoteComparisonPendingDto
                        {
                            ItemId       = r.ItemId,
                            Quantity     = r.Quantity,
                            UomId        = r.UomId,
                            HsnId        = r.HsnId,
                            Rate         = r.Rate,
                            Discount     = r.Discount,
                            GstPercent   = r.GstPercent,
                            GstAmount    = r.GstAmount,
                            Total        = r.Total,
                            SupplierId   = r.SupplierId,
                            SupplierName = r.SupplierName,

                            // these are enriched later in handler via gRPC:
                            ItemName     = r.ItemName,
                            UOM          = r.UOM,
                            HSNCode      = r.HSNCode
                        }).ToList()
                    };
                })
                .ToList();

            return (items, total);
        }
    }
}