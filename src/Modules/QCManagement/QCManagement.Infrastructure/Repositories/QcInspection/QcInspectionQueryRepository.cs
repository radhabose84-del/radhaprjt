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
        private readonly IArrivalLookup _arrivalLookup;
        private readonly IItemLookup _itemLookup;
        private readonly ISupplierLookup _supplierLookup;
        private readonly IInventoryCategoryLookup _categoryLookup;
        private readonly IOcrQualityParameterLookup _ocrQualityParameterLookup;

        public QcInspectionQueryRepository(
            IDbConnection dbConnection,
            IGrnLookup grnLookup,
            IArrivalLookup arrivalLookup,
            IItemLookup itemLookup,
            ISupplierLookup supplierLookup,
            IInventoryCategoryLookup categoryLookup,
            IOcrQualityParameterLookup ocrQualityParameterLookup)
        {
            _dbConnection = dbConnection;
            _grnLookup = grnLookup;
            _arrivalLookup = arrivalLookup;
            _itemLookup = itemLookup;
            _supplierLookup = supplierLookup;
            _categoryLookup = categoryLookup;
            _ocrQualityParameterLookup = ocrQualityParameterLookup;
        }

        public async Task<IReadOnlyList<QcInspectionSummaryDto>> GetInspectionSummariesBySourceAsync(int sourceTypeId, IEnumerable<int> sourceDetailIds)
        {
            var ids = sourceDetailIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<QcInspectionSummaryDto>();

            const string sql = @"
                SELECT
                    h.Id, h.SourceTypeId, h.SourceDetailId, h.QcInspectionNo,
                    h.QcStatusId, ms.Code AS QcStatusCode, ms.Description AS QcStatusName,
                    h.AcceptedQuantity, h.RejectedQuantity, h.InspectionDate,
                    h.CreatedDate, h.CreatedByName, h.ModifiedDate, h.ModifiedByName
                FROM QC.QcInspectionHdr h
                LEFT JOIN QC.MiscMaster ms ON h.QcStatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.SourceTypeId = @SourceTypeId AND h.SourceDetailId IN @Ids AND h.IsDeleted = 0";

            var rows = await _dbConnection.QueryAsync<QcInspectionSummaryDto>(sql, new { SourceTypeId = sourceTypeId, Ids = ids });
            return rows.ToList();
        }

        public async Task<int?> GetSourceTypeIdByCodeAsync(string sourceTypeCode)
        {
            const string sql = @"
                SELECT TOP 1 mm.Id
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mtm.MiscTypeCode = 'QP_SOURCE_TYPE'
                  AND mm.Code = @Code
                  AND mm.IsActive = 1 AND mm.IsDeleted = 0
                ORDER BY mm.Id ASC";
            return await _dbConnection.ExecuteScalarAsync<int?>(sql, new { Code = sourceTypeCode });
        }

        public async Task<QcInspectionDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    h.Id, h.QcInspectionNo, h.InspectionDate,
                    h.SourceTypeId, st.Code AS SourceTypeCode, st.Description AS SourceTypeName,
                    h.SourceHeaderId, h.SourceDetailId,
                    h.QualitySpecificationId, h.QualitySpecificationCode, h.QualityTemplateId, h.QualityTemplateCode, h.QcTypeId,
                    h.InspectorUserId, h.InspectorName, h.ReceivedQuantity, h.ReceivedUomId, h.BatchNumber, h.LotNumber,
                    h.QcStatusId, ms.Code AS QcStatusCode, ms.Description AS QcStatusName,
                    h.AcceptedQuantity, h.RejectedQuantity, h.DispositionRemarks, h.DispositionDate, h.DispositionByName,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName, h.CreatedIP, h.ModifiedBy, h.ModifiedDate, h.ModifiedByName, h.ModifiedIP
                FROM QC.QcInspectionHdr h
                LEFT JOIN QC.MiscMaster ms ON h.QcStatusId = ms.Id AND ms.IsDeleted = 0
                LEFT JOIN QC.MiscMaster st ON h.SourceTypeId = st.Id AND st.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0;

                SELECT
                    d.Id, d.QualitySpecificationParameterId, d.QualityParameterId, d.ParameterCode, d.ParameterName,
                    d.DataTypeId, d.ValidationTypeId, d.ValidationTypeCode, d.UomId, d.UomCode,
                    d.MinValue, d.MaxValue, d.ExpectedValue, d.AllowedValues AS AllowedValuesRaw,
                    d.SeverityId, d.SeverityCode, sev.Description AS SeverityName,
                    d.FailureActionId, fa.Code AS FailureActionCode, fa.Description AS FailureActionName,
                    d.SortOrder,
                    d.ActualValue, d.InspectionResult, d.Remarks
                FROM QC.QcInspectionDtl d
                LEFT JOIN QC.MiscMaster sev ON d.SeverityId = sev.Id
                LEFT JOIN QC.MiscMaster fa ON d.FailureActionId = fa.Id
                WHERE d.QcInspectionHdrId = @Id AND d.IsDeleted = 0
                ORDER BY d.SortOrder ASC, d.Id ASC;";

            QcInspectionDto? dto;
            List<dynamic> rows;
            // Scope the GridReader so the connection is free for the QualityParameter enrichment query below.
            using (var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id }))
            {
                dto = await multi.ReadFirstOrDefaultAsync<QcInspectionDto>();
                if (dto == null)
                    return null;

                rows = (await multi.ReadAsync<dynamic>()).ToList();
            }

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
                SeverityName = (string?)r.SeverityName,
                FailureActionId = (int)r.FailureActionId,
                FailureActionCode = (string?)r.FailureActionCode,
                FailureActionName = (string?)r.FailureActionName,
                SortOrder = (int)r.SortOrder,
                ActualValue = (string?)r.ActualValue,
                InspectionResult = (string?)r.InspectionResult,
                Remarks = (string?)r.Remarks
            }).ToList();

            // Resolve cross-module display fields — branch on the source document type.
            if (string.Equals(dto.SourceTypeCode, "ARRIVAL", StringComparison.OrdinalIgnoreCase))
            {
                var arrival = await _arrivalLookup.GetByArrivalDetailIdAsync(dto.SourceDetailId);
                if (arrival != null)
                {
                    dto.SourceNo = arrival.ArrivalNumber;
                    dto.SourceDate = arrival.ArrivalDate;
                    dto.InvoiceNo = null;
                    dto.SupplierId = arrival.SupplierId;
                    dto.ItemId = arrival.ItemId;
                }

                // OCR cotton-quality parameters captured on the OCR behind this Arrival
                // (ArrivalHeader → RawMaterialPOHeader → OCREntry → OCRQualityParameter),
                // enriched with the QC.QualityParameter master (code / name / data type).
                dto.OcrQualityParameters = await BuildOcrParametersAsync(dto.SourceHeaderId);

                // Surface the OCR-entered value inline on each inspection parameter row,
                // matched by QualityParameterId (= OCRQualityParameter.ParamId).
                if (dto.OcrQualityParameters.Count > 0)
                {
                    var ocrValueByParam = dto.OcrQualityParameters
                        .GroupBy(o => o.QualityParameterId)
                        .ToDictionary(g => g.Key, g => g.First().Value);

                    foreach (var p in dto.Parameters)
                    {
                        if (ocrValueByParam.TryGetValue(p.QualityParameterId, out var ocrValue))
                            p.OcrValue = ocrValue;
                    }
                }
            }
            else
            {
                var grn = await _grnLookup.GetByGrnDetailIdAsync(dto.SourceDetailId);
                if (grn != null)
                {
                    dto.SourceNo = grn.GrnNo;
                    dto.SourceDate = grn.GrnDate;
                    dto.InvoiceNo = grn.InvoiceNo;
                    dto.SupplierId = grn.SupplierId;
                    dto.ItemId = grn.ItemId;
                }
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

        // Resolves the OCR quality-parameter values for an Arrival and enriches each with the
        // QC.QualityParameter master (same-module). ParamId → QualityParameter.Id.
        private async Task<List<QcOcrQualityParameterDto>> BuildOcrParametersAsync(int arrivalHeaderId)
        {
            var ocrParams = await _ocrQualityParameterLookup.GetByArrivalHeaderIdAsync(arrivalHeaderId);
            if (ocrParams.Count == 0)
                return new List<QcOcrQualityParameterDto>();

            var paramIds = ocrParams.Select(p => p.ParamId).Distinct().ToList();

            const string sql = @"
                SELECT Id, ParameterCode, ParameterName, DataTypeId
                FROM QC.QualityParameter
                WHERE Id IN @Ids AND IsDeleted = 0";

            var masters = (await _dbConnection.QueryAsync<QualityParameterMasterRow>(sql, new { Ids = paramIds }))
                .ToDictionary(m => m.Id);

            return ocrParams.Select(p =>
            {
                masters.TryGetValue(p.ParamId, out var m);
                return new QcOcrQualityParameterDto
                {
                    QualityParameterId = p.ParamId,
                    ParameterCode = m?.ParameterCode,
                    ParameterName = m?.ParameterName,
                    DataTypeId = m?.DataTypeId ?? 0,
                    Value = p.Value
                };
            }).ToList();
        }

        // Minimal projection of QC.QualityParameter used to enrich OCR quality parameters.
        private sealed class QualityParameterMasterRow
        {
            public int Id { get; set; }
            public string? ParameterCode { get; set; }
            public string? ParameterName { get; set; }
            public int DataTypeId { get; set; }
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM QC.QcInspectionHdr WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id }) == 0;
        }

        public async Task<bool> InspectionExistsForSourceAsync(int sourceTypeId, int sourceDetailId)
        {
            const string sql = "SELECT COUNT(1) FROM QC.QcInspectionHdr WHERE SourceTypeId = @SourceTypeId AND SourceDetailId = @SourceDetailId AND IsDeleted = 0";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { SourceTypeId = sourceTypeId, SourceDetailId = sourceDetailId }) > 0;
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

        public async Task<bool> QcStatusIdExistsAsync(int qcStatusId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mm.Id = @Id
                  AND mm.IsActive = 1 AND mm.IsDeleted = 0
                  AND mtm.IsDeleted = 0
                  AND mtm.MiscTypeCode = 'QP_QC_STATUS'";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = qcStatusId }) > 0;
        }

        public async Task<int?> GetQcStatusIdByCodeAsync(string statusCode)
        {
            const string sql = @"
                SELECT TOP 1 mm.Id
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mtm.MiscTypeCode = 'QP_QC_STATUS'
                  AND mm.Code = @Code
                  AND mm.IsActive = 1 AND mm.IsDeleted = 0
                  AND mtm.IsDeleted = 0
                ORDER BY mm.Id ASC";
            return await _dbConnection.ExecuteScalarAsync<int?>(sql, new { Code = statusCode });
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

        public async Task<string?> GetQcStatusCodeByIdAsync(int qcStatusId)
        {
            const string sql = @"
                SELECT TOP 1 mm.Code
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mm.Id = @Id AND mm.IsDeleted = 0
                  AND mtm.MiscTypeCode = 'QP_QC_STATUS'";
            return await _dbConnection.ExecuteScalarAsync<string?>(sql, new { Id = qcStatusId });
        }

        public async Task<IReadOnlyList<QcInspectionDtlEvalDto>> GetDetailEvaluationRowsAsync(int qcInspectionHdrId)
        {
            const string sql = @"
                SELECT Id, ValidationTypeCode, MinValue, MaxValue, ExpectedValue, AllowedValues, SeverityCode
                FROM QC.QcInspectionDtl
                WHERE QcInspectionHdrId = @Id AND IsDeleted = 0";
            var rows = await _dbConnection.QueryAsync<QcInspectionDtlEvalDto>(sql, new { Id = qcInspectionHdrId });
            return rows.ToList();
        }

        public async Task<QcDispositionContextDto?> GetDispositionContextAsync(int id)
        {
            const string sql = @"
                SELECT SourceTypeId, SourceHeaderId, SourceDetailId, ReceivedQuantity, ReceivedUomId, QcInspectionNo
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
                WHERE h.SourceHeaderId = @Id AND h.IsDeleted = 0 AND h.QcStatusId IS NOT NULL
                  AND h.SourceTypeId = (
                      SELECT TOP 1 mm.Id FROM QC.MiscMaster mm
                      INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                      WHERE mtm.MiscTypeCode = 'QP_SOURCE_TYPE' AND mm.Code = 'GRN'
                        AND mm.IsActive = 1 AND mm.IsDeleted = 0)";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<GrnQcStatusDto>(sql, new { Id = grnHeaderId });
            return result ?? new GrnQcStatusDto { GrnHeaderId = grnHeaderId };
        }
    }
}
