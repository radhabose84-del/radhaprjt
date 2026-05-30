using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Dapper;
using QCManagement.Application.Common.Interfaces.IQcInspection;
using QCManagement.Application.QcInspection.Dto;

namespace QCManagement.Infrastructure.Repositories.QcInspection
{
    public class QcInspectionQueryRepository : IQcInspectionQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IGrnLookup _grnLookup;
        private readonly IItemLookup _itemLookup;
        private readonly ISupplierLookup _supplierLookup;
        private readonly IInventoryCategoryLookup _categoryLookup;

        public QcInspectionQueryRepository(
            IDbConnection dbConnection,
            IGrnLookup grnLookup,
            IItemLookup itemLookup,
            ISupplierLookup supplierLookup,
            IInventoryCategoryLookup categoryLookup)
        {
            _dbConnection = dbConnection;
            _grnLookup = grnLookup;
            _itemLookup = itemLookup;
            _supplierLookup = supplierLookup;
            _categoryLookup = categoryLookup;
        }

        public async Task<(List<QcInspectionListDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm,
            int? qcStatusId, DateTimeOffset? fromDate, DateTimeOffset? toDate)
        {
            var where = "h.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                where += " AND (h.QcInspectionNo LIKE @Search OR h.BatchNumber LIKE @Search)";
            if (qcStatusId.HasValue)
                where += " AND h.QcStatusId = @QcStatusId";
            if (fromDate.HasValue)
                where += " AND h.InspectionDate >= @FromDate";
            if (toDate.HasValue)
                where += " AND h.InspectionDate <= @ToDate";

            var sql = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) FROM QC.QcInspectionHdr h WHERE {where};

                SELECT
                    h.Id, h.QcInspectionNo, h.GrnHeaderId, h.GrnDetailId, h.BatchNumber,
                    h.ReceivedQuantity, h.AcceptedQuantity, h.RejectedQuantity,
                    h.QcStatusId, ms.Code AS QcStatusCode, ms.Description AS QcStatusName,
                    h.InspectionDate, h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM QC.QcInspectionHdr h
                LEFT JOIN QC.MiscMaster ms ON h.QcStatusId = ms.Id AND ms.IsDeleted = 0
                WHERE {where}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                QcStatusId = qcStatusId,
                FromDate = fromDate,
                ToDate = toDate,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var list = (await multi.ReadAsync<QcInspectionListDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                // Resolve GRN No / Supplier / Item via cross-module lookups (no JOIN).
                var grnDict = (await _grnLookup.GetByGrnDetailIdsAsync(list.Select(x => x.GrnDetailId)))
                    .GroupBy(g => g.GrnDetailId)
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var row in list)
                {
                    if (grnDict.TryGetValue(row.GrnDetailId, out var grn))
                    {
                        row.GrnNo = grn.GrnNo;
                        row.SupplierId = grn.SupplierId;
                        row.ItemId = grn.ItemId;
                    }
                }

                var itemIds = list.Where(x => x.ItemId > 0).Select(x => x.ItemId).Distinct().ToList();
                var itemDict = itemIds.Count > 0
                    ? (await _itemLookup.GetByIdsAsync(itemIds)).ToDictionary(i => i.Id)
                    : new();

                var supplierNames = new Dictionary<int, string?>();
                foreach (var sid in list.Where(x => x.SupplierId > 0).Select(x => x.SupplierId).Distinct())
                {
                    var s = await _supplierLookup.GetActiveSupplierByIdAsync(sid);
                    supplierNames[sid] = s?.VendorName;
                }

                foreach (var row in list)
                {
                    if (row.ItemId > 0 && itemDict.TryGetValue(row.ItemId, out var item))
                        row.ItemName = item.ItemName;
                    if (row.SupplierId > 0 && supplierNames.TryGetValue(row.SupplierId, out var sn))
                        row.SupplierName = sn;
                }
            }

            return (list, totalCount);
        }

        public async Task<QcInspectionDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    h.Id, h.QcInspectionNo, h.InspectionDate, h.GrnHeaderId, h.GrnDetailId,
                    h.QualitySpecificationId, h.QualitySpecificationCode, h.QualityTemplateId, h.QualityTemplateCode, h.QcTypeId,
                    h.InspectorUserId, h.InspectorName, h.ReceivedQuantity, h.ReceivedUomId, h.BatchNumber, h.LotNumber,
                    h.QcStatusId, ms.Code AS QcStatusCode, ms.Description AS QcStatusName,
                    h.AcceptedQuantity, h.RejectedQuantity, h.DispositionRemarks, h.DispositionDate, h.DispositionByName,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName, h.CreatedIP, h.ModifiedBy, h.ModifiedDate, h.ModifiedByName, h.ModifiedIP
                FROM QC.QcInspectionHdr h
                LEFT JOIN QC.MiscMaster ms ON h.QcStatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0;

                SELECT
                    d.Id, d.QualitySpecificationParameterId, d.QualityParameterId, d.ParameterCode, d.ParameterName,
                    d.DataTypeId, d.ValidationTypeId, d.ValidationTypeCode, d.UomId, d.UomCode,
                    d.MinValue, d.MaxValue, d.ExpectedValue, d.AllowedValues AS AllowedValuesRaw,
                    d.SeverityId, d.SeverityCode, d.FailureActionId, d.SortOrder,
                    d.ActualValue, d.InspectionResult, d.Remarks
                FROM QC.QcInspectionDtl d
                WHERE d.QcInspectionHdrId = @Id AND d.IsDeleted = 0
                ORDER BY d.SortOrder ASC, d.Id ASC;";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id });

            var dto = await multi.ReadFirstOrDefaultAsync<QcInspectionDto>();
            if (dto == null)
                return null;

            var rows = (await multi.ReadAsync<dynamic>()).ToList();
            dto.Parameters = rows.Select(r => new QcInspectionParameterResultDto
            {
                Id = (int)r.Id,
                QualitySpecificationParameterId = (int)r.QualitySpecificationParameterId,
                QualityParameterId = (int)r.QualityParameterId,
                ParameterCode = (string?)r.ParameterCode,
                ParameterName = (string?)r.ParameterName,
                DataTypeId = (int)r.DataTypeId,
                ValidationTypeId = (int)r.ValidationTypeId,
                ValidationTypeCode = (string?)r.ValidationTypeCode,
                UomId = (int?)r.UomId,
                UomCode = (string?)r.UomCode,
                MinValue = (decimal?)r.MinValue,
                MaxValue = (decimal?)r.MaxValue,
                ExpectedValue = (string?)r.ExpectedValue,
                AllowedValues = string.IsNullOrWhiteSpace((string?)r.AllowedValuesRaw)
                    ? new List<string>()
                    : ((string)r.AllowedValuesRaw).Split('|', StringSplitOptions.RemoveEmptyEntries).ToList(),
                SeverityId = (int)r.SeverityId,
                SeverityCode = (string?)r.SeverityCode,
                FailureActionId = (int)r.FailureActionId,
                SortOrder = (int)r.SortOrder,
                ActualValue = (string?)r.ActualValue,
                InspectionResult = (string?)r.InspectionResult,
                Remarks = (string?)r.Remarks
            }).ToList();

            // Resolve cross-module display fields
            var grn = await _grnLookup.GetByGrnDetailIdAsync(dto.GrnDetailId);
            if (grn != null)
            {
                dto.GrnNo = grn.GrnNo;
                dto.GrnDate = grn.GrnDate;
                dto.InvoiceNo = grn.InvoiceNo;
                dto.SupplierId = grn.SupplierId;
                dto.ItemId = grn.ItemId;
            }

            if (dto.SupplierId > 0)
            {
                var s = await _supplierLookup.GetActiveSupplierByIdAsync(dto.SupplierId);
                dto.SupplierName = s?.VendorName;
            }

            if (dto.ItemId > 0)
            {
                var item = (await _itemLookup.GetByIdsAsync(new[] { dto.ItemId })).FirstOrDefault();
                if (item != null)
                {
                    dto.ItemCode = item.ItemCode;
                    dto.ItemName = item.ItemName;
                    dto.ItemCategoryId = item.ItemCategoryId;
                }
            }

            if (dto.ItemCategoryId.HasValue && dto.ItemCategoryId.Value > 0)
            {
                var cat = (await _categoryLookup.GetCategoryByIdsAsync(new[] { dto.ItemCategoryId.Value })).FirstOrDefault();
                dto.ItemCategoryName = cat?.ItemCategoryName;
            }

            return dto;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM QC.QcInspectionHdr WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id }) == 0;
        }

        public async Task<bool> InspectionExistsForGrnDetailAsync(int grnDetailId)
        {
            const string sql = "SELECT COUNT(1) FROM QC.QcInspectionHdr WHERE GrnDetailId = @Id AND IsDeleted = 0";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = grnDetailId }) > 0;
        }

        public async Task<bool> IsDisposedAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM QC.QcInspectionHdr WHERE Id = @Id AND QcStatusId IS NOT NULL AND IsDeleted = 0";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id }) > 0;
        }

        public async Task<bool> AllParametersEvaluatedAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM QC.QcInspectionDtl
                WHERE QcInspectionHdrId = @Id AND IsDeleted = 0 AND InspectionResult IS NULL";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id }) == 0;
        }

        public async Task<bool> HasCriticalFailureAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM QC.QcInspectionDtl
                WHERE QcInspectionHdrId = @Id AND IsDeleted = 0
                  AND SeverityCode = 'CRT' AND InspectionResult = 'FAIL'";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id }) > 0;
        }

        public async Task<decimal?> GetReceivedQuantityAsync(int id)
        {
            const string sql = "SELECT ReceivedQuantity FROM QC.QcInspectionHdr WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.ExecuteScalarAsync<decimal?>(sql, new { Id = id });
        }

        public async Task<bool> DetailBelongsToHeaderAsync(int detailId, int headerId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM QC.QcInspectionDtl
                WHERE Id = @DetailId AND QcInspectionHdrId = @HeaderId AND IsDeleted = 0";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { DetailId = detailId, HeaderId = headerId }) > 0;
        }

        public async Task<bool> QcStatusCodeExistsAsync(string qcStatusCode)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mm.Code = @Code
                  AND mm.IsActive = 1 AND mm.IsDeleted = 0
                  AND mtm.IsDeleted = 0
                  AND mtm.MiscTypeCode = 'QP_QC_STATUS'";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Code = qcStatusCode }) > 0;
        }

        public async Task<int> GetMaxInspectionSequenceAsync(int year)
        {
            const string sql = @"
                SELECT ISNULL(MAX(CAST(SUBSTRING(QcInspectionNo, 10, 5) AS INT)), 0)
                FROM QC.QcInspectionHdr
                WHERE QcInspectionNo LIKE @Pattern";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Pattern = $"QCI-{year}-%" });
        }

        public async Task<int?> GetPurchasedGoodsQcTypeIdAsync()
        {
            const string sql = @"
                SELECT TOP 1 mm.Id
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mtm.MiscTypeCode = 'QP_QC_TYPE'
                  AND mm.IsActive = 1 AND mm.IsDeleted = 0
                  AND (UPPER(mm.Code) LIKE '%PURCHAS%' OR UPPER(mm.Description) LIKE '%PURCHAS%')
                ORDER BY mm.Id ASC";
            return await _dbConnection.ExecuteScalarAsync<int?>(sql);
        }

        public async Task<int?> ResolveActiveSpecIdAsync(int itemId, int? itemCategoryId, int qcTypeId, DateTimeOffset asOfDate)
        {
            const string sql = @"
                ;WITH ItemMatch AS (
                    SELECT TOP 1 qs.Id, 1 AS Priority, qs.EffectiveFrom
                    FROM QC.QualitySpecification qs
                    WHERE qs.IsDeleted = 0 AND qs.IsActive = 1 AND qs.QcTypeId = @QcTypeId
                      AND qs.ItemId = @ItemId
                      AND @AsOf BETWEEN qs.EffectiveFrom AND ISNULL(qs.EffectiveTo, '9999-12-31')
                    ORDER BY qs.EffectiveFrom DESC
                ),
                CategoryMatch AS (
                    SELECT TOP 1 qs.Id, 2 AS Priority, qs.EffectiveFrom
                    FROM QC.QualitySpecification qs
                    WHERE qs.IsDeleted = 0 AND qs.IsActive = 1 AND qs.QcTypeId = @QcTypeId
                      AND qs.ItemId IS NULL AND qs.ItemCategoryId = @ItemCategoryId
                      AND @AsOf BETWEEN qs.EffectiveFrom AND ISNULL(qs.EffectiveTo, '9999-12-31')
                    ORDER BY qs.EffectiveFrom DESC
                )
                SELECT TOP 1 Id FROM (
                    SELECT Id, Priority FROM ItemMatch
                    UNION ALL
                    SELECT Id, Priority FROM CategoryMatch
                ) m
                ORDER BY Priority ASC";

            return await _dbConnection.ExecuteScalarAsync<int?>(sql, new
            {
                ItemId = itemId,
                ItemCategoryId = itemCategoryId,
                QcTypeId = qcTypeId,
                AsOf = asOfDate
            });
        }

        public async Task<QcSpecSnapshotDto?> GetSpecSnapshotAsync(int qualitySpecificationId)
        {
            const string sql = @"
                SELECT
                    qs.Id AS QualitySpecificationId,
                    qs.SpecificationCode AS QualitySpecificationCode,
                    qs.QualityTemplateId,
                    qt.TemplateCode AS QualityTemplateCode,
                    qs.QcTypeId
                FROM QC.QualitySpecification qs
                LEFT JOIN QC.QualityTemplate qt ON qs.QualityTemplateId = qt.Id AND qt.IsDeleted = 0
                WHERE qs.Id = @Id AND qs.IsDeleted = 0;

                SELECT
                    qsp.Id AS QualitySpecificationParameterId,
                    qsp.QualityParameterId,
                    qp.ParameterCode,
                    qp.ParameterName,
                    ISNULL(qp.DataTypeId, 0) AS DataTypeId,
                    qsp.ValidationTypeId,
                    vt.Code AS ValidationTypeCode,
                    CAST(NULL AS INT) AS UomId,
                    CAST(NULL AS VARCHAR(20)) AS UomCode,
                    qsp.MinValue, qsp.MaxValue, qsp.ExpectedValue,
                    qsp.AllowedValues,
                    ISNULL(qsp.SeverityId, 0) AS SeverityId,
                    ISNULL(sv.Code, '') AS SeverityCode,
                    ISNULL(qsp.FailureActionId, 0) AS FailureActionId,
                    ROW_NUMBER() OVER (ORDER BY qsp.Id ASC) AS SortOrder
                FROM QC.QualitySpecificationParameter qsp
                LEFT JOIN QC.QualityParameter qp ON qsp.QualityParameterId = qp.Id AND qp.IsDeleted = 0
                LEFT JOIN QC.MiscMaster vt ON qsp.ValidationTypeId = vt.Id AND vt.IsDeleted = 0
                LEFT JOIN QC.MiscMaster sv ON qsp.SeverityId = sv.Id AND sv.IsDeleted = 0
                WHERE qsp.QualitySpecificationId = @Id AND qsp.IsDeleted = 0
                ORDER BY qsp.Id ASC;";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = qualitySpecificationId });

            var header = await multi.ReadFirstOrDefaultAsync<QcSpecSnapshotDto>();
            if (header == null)
                return null;

            header.Parameters = (await multi.ReadAsync<QcSpecParamSnapshotDto>()).ToList();
            return header;
        }

        public async Task<int?> GetQcStatusIdByCodeAsync(string qcStatusCode)
        {
            const string sql = @"
                SELECT TOP 1 mm.Id
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mm.Code = @Code AND mm.IsDeleted = 0
                  AND mtm.MiscTypeCode = 'QP_QC_STATUS'";
            return await _dbConnection.ExecuteScalarAsync<int?>(sql, new { Code = qcStatusCode });
        }

        public async Task<IReadOnlyList<QcInspectionDtlEvalDto>> GetDetailEvaluationRowsAsync(int qcInspectionHdrId)
        {
            const string sql = @"
                SELECT Id, ValidationTypeCode, MinValue, MaxValue, ExpectedValue, AllowedValues
                FROM QC.QcInspectionDtl
                WHERE QcInspectionHdrId = @Id AND IsDeleted = 0";
            var rows = await _dbConnection.QueryAsync<QcInspectionDtlEvalDto>(sql, new { Id = qcInspectionHdrId });
            return rows.ToList();
        }

        public async Task<QcDispositionContextDto?> GetDispositionContextAsync(int id)
        {
            const string sql = @"
                SELECT GrnHeaderId, GrnDetailId, ReceivedQuantity, ReceivedUomId, QcInspectionNo
                FROM QC.QcInspectionHdr
                WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<QcDispositionContextDto>(sql, new { Id = id });
        }

        public async Task<GrnQcStatusDto> GetGrnStatusCountsAsync(int grnHeaderId)
        {
            const string sql = @"
                SELECT
                    COUNT(*) AS InspectedCount,
                    SUM(CASE WHEN ms.Code = 'APR' THEN 1 ELSE 0 END) AS ApprovedCount,
                    SUM(CASE WHEN ms.Code = 'CAP' THEN 1 ELSE 0 END) AS ConditionallyApprovedCount,
                    SUM(CASE WHEN ms.Code = 'HLD' THEN 1 ELSE 0 END) AS HoldCount,
                    SUM(CASE WHEN ms.Code = 'RJC' THEN 1 ELSE 0 END) AS RejectedCount
                FROM QC.QcInspectionHdr h
                LEFT JOIN QC.MiscMaster ms ON h.QcStatusId = ms.Id
                WHERE h.GrnHeaderId = @Id AND h.IsDeleted = 0 AND h.QcStatusId IS NOT NULL";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<GrnQcStatusDto>(sql, new { Id = grnHeaderId });
            return result ?? new GrnQcStatusDto { GrnHeaderId = grnHeaderId };
        }

        public async Task<IReadOnlyList<int>> GetInspectedGrnDetailIdsAsync(IEnumerable<int> grnDetailIds)
        {
            var ids = grnDetailIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<int>();

            const string sql = @"
                SELECT GrnDetailId FROM QC.QcInspectionHdr
                WHERE GrnDetailId IN @Ids AND IsDeleted = 0";
            var rows = await _dbConnection.QueryAsync<int>(sql, new { Ids = ids });
            return rows.ToList();
        }
    }
}
