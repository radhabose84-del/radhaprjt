using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;
using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Application.QualitySpecification.Dto;

namespace QCManagement.Infrastructure.Repositories.QualitySpecification
{
    public class QualitySpecificationQueryRepository : IQualitySpecificationQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IInventoryCategoryLookup _categoryLookup;
        private readonly IItemLookup _itemLookup;

        public QualitySpecificationQueryRepository(
            IDbConnection dbConnection,
            IInventoryCategoryLookup categoryLookup,
            IItemLookup itemLookup)
        {
            _dbConnection = dbConnection;
            _categoryLookup = categoryLookup;
            _itemLookup = itemLookup;
        }

        public async Task<(List<QualitySpecificationListDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm,
            int? qualityTemplateId, int? applicableLevelId,
            int? itemCategoryId, int? itemId, bool? isActive)
        {
            var whereClause = "qs.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (qs.SpecificationCode LIKE @Search OR qs.SpecificationName LIKE @Search)";
            if (qualityTemplateId.HasValue)
                whereClause += " AND qs.QualityTemplateId = @QualityTemplateId";
            if (applicableLevelId.HasValue)
                whereClause += " AND qs.ApplicableLevelId = @ApplicableLevelId";
            if (itemCategoryId.HasValue)
                whereClause += " AND qs.ItemCategoryId = @ItemCategoryId";
            if (itemId.HasValue)
                whereClause += " AND qs.ItemId = @ItemId";
            if (isActive.HasValue)
                whereClause += " AND qs.IsActive = @IsActive";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM QC.QualitySpecification qs
                WHERE {whereClause};

                SELECT
                    qs.Id, qs.SpecificationCode, qs.SpecificationName,
                    qs.QualityTemplateId,
                    qt.TemplateName AS QualityTemplateName,
                    qs.ApplicableLevelId,
                    al.Description AS ApplicableLevelName,
                    qs.ItemCategoryId, qs.ItemId,
                    qs.EffectiveFrom, qs.EffectiveTo,
                    qs.IsActive, qs.IsDeleted,
                    qs.CreatedBy, qs.CreatedDate, qs.CreatedByName, qs.CreatedIP,
                    qs.ModifiedBy, qs.ModifiedDate, qs.ModifiedByName, qs.ModifiedIP,
                    ISNULL((
                        SELECT COUNT(*) FROM QC.QualitySpecificationParameter qsp
                        WHERE qsp.QualitySpecificationId = qs.Id AND qsp.IsDeleted = 0
                    ), 0) AS ParameterCount
                FROM QC.QualitySpecification qs
                LEFT JOIN QC.QualityTemplate qt ON qs.QualityTemplateId = qt.Id AND qt.IsDeleted = 0
                LEFT JOIN QC.MiscMaster al ON qs.ApplicableLevelId = al.Id AND al.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY qs.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                QualityTemplateId = qualityTemplateId,
                ApplicableLevelId = applicableLevelId,
                ItemCategoryId = itemCategoryId,
                ItemId = itemId,
                IsActive = isActive,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<QualitySpecificationListDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            // Populate AppliesTo via cross-module lookups
            var categoryIds = list
                .Where(x => x.ItemCategoryId.HasValue && x.ItemCategoryId.Value > 0)
                .Select(x => x.ItemCategoryId!.Value)
                .Distinct()
                .ToList();

            var itemIds = list
                .Where(x => x.ItemId.HasValue && x.ItemId.Value > 0)
                .Select(x => x.ItemId!.Value)
                .Distinct()
                .ToList();

            var categoryDict = categoryIds.Count > 0
                ? (await _categoryLookup.GetCategoryByIdsAsync(categoryIds)).ToDictionary(c => c.Id)
                : new();

            var itemDict = itemIds.Count > 0
                ? (await _itemLookup.GetByIdsAsync(itemIds)).ToDictionary(i => i.Id)
                : new();

            foreach (var row in list)
            {
                if (row.ItemId.HasValue && itemDict.TryGetValue(row.ItemId.Value, out var item))
                    row.AppliesTo = item.ItemName;
                else if (row.ItemCategoryId.HasValue && categoryDict.TryGetValue(row.ItemCategoryId.Value, out var cat))
                    row.AppliesTo = cat.ItemCategoryName;
            }

            return (list, totalCount);
        }

        public async Task<QualitySpecificationDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    qs.Id, qs.SpecificationCode, qs.SpecificationName,
                    qs.QualityTemplateId,
                    qt.TemplateCode AS QualityTemplateCode,
                    qt.TemplateName AS QualityTemplateName,
                    qs.ApplicableLevelId,
                    al.Code AS ApplicableLevelCode,
                    al.Description AS ApplicableLevelName,
                    qs.ItemCategoryId, qs.ItemId,
                    qs.Description,
                    qs.EffectiveFrom, qs.EffectiveTo,
                    qs.IsActive, qs.IsDeleted,
                    qs.CreatedBy, qs.CreatedDate, qs.CreatedByName, qs.CreatedIP,
                    qs.ModifiedBy, qs.ModifiedDate, qs.ModifiedByName, qs.ModifiedIP
                FROM QC.QualitySpecification qs
                LEFT JOIN QC.QualityTemplate qt ON qs.QualityTemplateId = qt.Id AND qt.IsDeleted = 0
                LEFT JOIN QC.MiscMaster al ON qs.ApplicableLevelId = al.Id AND al.IsDeleted = 0
                WHERE qs.Id = @Id AND qs.IsDeleted = 0;

                SELECT
                    qsp.Id, qsp.QualityParameterId,
                    qp.ParameterCode, qp.ParameterName,
                    dt.Code AS ParameterDataTypeCode,
                    qsp.ValidationTypeId,
                    vt.Code AS ValidationTypeCode,
                    vt.Description AS ValidationTypeName,
                    qsp.MinValue, qsp.MaxValue, qsp.ExpectedValue,
                    qsp.AllowedValues AS AllowedValuesRaw,
                    qsp.SeverityId,
                    sv.Code AS SeverityCode,
                    sv.Description AS SeverityName,
                    qsp.FailureActionId,
                    fa.Code AS FailureActionCode,
                    fa.Description AS FailureActionName,
                    qsp.IsSamplingRequired, qsp.Remarks, qsp.IsActive
                FROM QC.QualitySpecificationParameter qsp
                LEFT JOIN QC.QualityParameter qp ON qsp.QualityParameterId = qp.Id AND qp.IsDeleted = 0
                LEFT JOIN QC.MiscMaster dt ON qp.DataTypeId = dt.Id AND dt.IsDeleted = 0
                LEFT JOIN QC.MiscMaster vt ON qsp.ValidationTypeId = vt.Id AND vt.IsDeleted = 0
                LEFT JOIN QC.MiscMaster sv ON qsp.SeverityId = sv.Id AND sv.IsDeleted = 0
                LEFT JOIN QC.MiscMaster fa ON qsp.FailureActionId = fa.Id AND fa.IsDeleted = 0
                WHERE qsp.QualitySpecificationId = @Id AND qsp.IsDeleted = 0
                ORDER BY qsp.Id ASC;";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id });

            var dto = await multi.ReadFirstOrDefaultAsync<QualitySpecificationDto>();
            if (dto == null)
                return null;

            var rows = (await multi.ReadAsync<dynamic>()).ToList();
            dto.Parameters = rows.Select(r => new QualitySpecificationParameterDto
            {
                Id = (int)r.Id,
                QualityParameterId = (int)r.QualityParameterId,
                ParameterCode = (string?)r.ParameterCode,
                ParameterName = (string?)r.ParameterName,
                ParameterDataTypeCode = (string?)r.ParameterDataTypeCode,
                ValidationTypeId = (int)r.ValidationTypeId,
                ValidationTypeCode = (string?)r.ValidationTypeCode,
                ValidationTypeName = (string?)r.ValidationTypeName,
                MinValue = (decimal?)r.MinValue,
                MaxValue = (decimal?)r.MaxValue,
                ExpectedValue = (string?)r.ExpectedValue,
                AllowedValues = string.IsNullOrWhiteSpace((string?)r.AllowedValuesRaw)
                    ? new List<string>()
                    : ((string)r.AllowedValuesRaw).Split('|', StringSplitOptions.RemoveEmptyEntries).ToList(),
                SeverityId = (int?)r.SeverityId,
                SeverityCode = (string?)r.SeverityCode,
                SeverityName = (string?)r.SeverityName,
                FailureActionId = (int?)r.FailureActionId,
                FailureActionCode = (string?)r.FailureActionCode,
                FailureActionName = (string?)r.FailureActionName,
                IsSamplingRequired = (bool)r.IsSamplingRequired,
                Remarks = (string?)r.Remarks,
                IsActive = (bool)r.IsActive
            }).ToList();

            // Populate ItemCategoryName / ItemCode / ItemName via cross-module lookup
            if (dto.ItemCategoryId.HasValue && dto.ItemCategoryId.Value > 0)
            {
                var categories = await _categoryLookup.GetCategoryByIdsAsync(new[] { dto.ItemCategoryId.Value });
                var cat = categories.FirstOrDefault();
                if (cat != null)
                    dto.ItemCategoryName = cat.ItemCategoryName;
            }

            if (dto.ItemId.HasValue && dto.ItemId.Value > 0)
            {
                var items = await _itemLookup.GetByIdsAsync(new[] { dto.ItemId.Value });
                var item = items.FirstOrDefault();
                if (item != null)
                {
                    dto.ItemCode = item.ItemCode;
                    dto.ItemName = item.ItemName;
                }
            }

            return dto;
        }

        public async Task<IReadOnlyList<QualitySpecificationLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, SpecificationCode, SpecificationName
                FROM QC.QualitySpecification
                WHERE IsActive = 1 AND IsDeleted = 0
                AND (@Term = '' OR SpecificationCode LIKE '%' + @Term + '%' OR SpecificationName LIKE '%' + @Term + '%')
                ORDER BY SpecificationCode ASC";

            var result = await _dbConnection.QueryAsync<QualitySpecificationLookupDto>(
                new CommandDefinition(sql, new { Term = term ?? string.Empty }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string specificationName, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM QC.QualitySpecification
                WHERE LOWER(SpecificationName) = LOWER(@Name)
                AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Name = specificationName.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM QC.QualitySpecification
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> ApplicableLevelExistsAsync(int applicableLevelId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mm.Id = @Id
                AND mm.IsActive = 1 AND mm.IsDeleted = 0
                AND mtm.IsDeleted = 0
                AND mtm.MiscTypeCode = 'QP_APPLICABLE_LEVEL'";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = applicableLevelId });
            return count > 0;
        }

        public async Task<bool> ValidationTypeExistsAsync(int validationTypeId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mm.Id = @Id
                AND mm.IsActive = 1 AND mm.IsDeleted = 0
                AND mtm.IsDeleted = 0
                AND mtm.MiscTypeCode = 'QP_VALIDATION'";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = validationTypeId });
            return count > 0;
        }

        public async Task<bool> SeverityExistsAsync(int severityId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mm.Id = @Id
                AND mm.IsActive = 1 AND mm.IsDeleted = 0
                AND mtm.IsDeleted = 0
                AND mtm.MiscTypeCode = 'QP_SEVERITY'";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = severityId });
            return count > 0;
        }

        public async Task<bool> FailureActionExistsAsync(int failureActionId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mm.Id = @Id
                AND mm.IsActive = 1 AND mm.IsDeleted = 0
                AND mtm.IsDeleted = 0
                AND mtm.MiscTypeCode = 'QP_FAILURE_ACTION'";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = failureActionId });
            return count > 0;
        }

        public async Task<string?> GetApplicableLevelCodeAsync(int applicableLevelId)
        {
            const string sql = @"
                SELECT mm.Code
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mm.Id = @Id
                AND mm.IsDeleted = 0
                AND mtm.MiscTypeCode = 'QP_APPLICABLE_LEVEL'";

            return await _dbConnection.ExecuteScalarAsync<string?>(sql, new { Id = applicableLevelId });
        }

        public async Task<Dictionary<int, string>> GetValidationTypeCodesByIdsAsync(IEnumerable<int> validationTypeIds)
        {
            var ids = validationTypeIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<int, string>();

            const string sql = @"
                SELECT mm.Id, mm.Code
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mm.Id IN @Ids
                AND mm.IsDeleted = 0
                AND mtm.IsDeleted = 0
                AND mtm.MiscTypeCode = 'QP_VALIDATION'";

            var rows = await _dbConnection.QueryAsync<(int Id, string Code)>(sql, new { Ids = ids });
            return rows.ToDictionary(r => r.Id, r => r.Code);
        }

        public async Task<List<int>> GetExistingParameterRowIdsAsync(int qualitySpecificationId)
        {
            const string sql = @"
                SELECT Id
                FROM QC.QualitySpecificationParameter
                WHERE QualitySpecificationId = @Id AND IsDeleted = 0";

            var result = await _dbConnection.QueryAsync<int>(sql, new { Id = qualitySpecificationId });
            return result.ToList();
        }

        public async Task<(int? ItemCategoryId, int? ItemId)> GetSpecificationItemContextAsync(int id)
        {
            const string sql = @"
                SELECT ItemCategoryId, ItemId
                FROM QC.QualitySpecification
                WHERE Id = @Id AND IsDeleted = 0";

            var row = await _dbConnection.QueryFirstOrDefaultAsync<(int? ItemCategoryId, int? ItemId)>(sql, new { Id = id });
            return row;
        }

        public async Task<bool> HasOverlappingActiveSpecAsync(
            int? itemCategoryId, int? itemId,
            DateTimeOffset effectiveFrom, DateTimeOffset? effectiveTo,
            int? excludeSpecId = null)
        {
            // Open-ended date handled via COALESCE to far-future date
            var sql = @"
                SELECT COUNT(1)
                FROM QC.QualitySpecification
                WHERE IsActive = 1 AND IsDeleted = 0
                AND ((@ItemId IS NOT NULL AND ItemId = @ItemId)
                  OR (@ItemCategoryId IS NOT NULL AND ItemCategoryId = @ItemCategoryId))
                AND (
                    @EffectiveFrom <= COALESCE(EffectiveTo, '9999-12-31')
                    AND COALESCE(@EffectiveTo, '9999-12-31') >= EffectiveFrom
                )";

            if (excludeSpecId.HasValue && excludeSpecId.Value > 0)
                sql += " AND Id != @ExcludeId";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                ItemId = itemId,
                ItemCategoryId = itemCategoryId,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = effectiveTo,
                ExcludeId = excludeSpecId
            });
            return count > 0;
        }

        public async Task<int> GetMaxSpecificationCodeSequenceAsync()
        {
            // Extract numeric suffix from QS-0001 format and return the max value.
            const string sql = @"
                SELECT ISNULL(MAX(CAST(SUBSTRING(SpecificationCode, 4, 10) AS INT)), 0)
                FROM QC.QualitySpecification
                WHERE SpecificationCode LIKE 'QS-[0-9]%'";

            return await _dbConnection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // No dependents yet — QC Inspection (future) will extend this check.
            return await Task.FromResult(false);
        }
    }
}
