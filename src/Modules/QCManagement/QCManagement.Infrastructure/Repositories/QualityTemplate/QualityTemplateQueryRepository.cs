using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.QualityTemplate.Dto;

namespace QCManagement.Infrastructure.Repositories.QualityTemplate
{
    public class QualityTemplateQueryRepository : IQualityTemplateQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUOMLookup _uomLookup;

        public QualityTemplateQueryRepository(IDbConnection dbConnection, IUOMLookup uomLookup)
        {
            _dbConnection = dbConnection;
            _uomLookup = uomLookup;
        }

        public async Task<(List<QualityTemplateListDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, bool? isActive = null)
        {
            var whereClause = "qt.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (qt.TemplateCode LIKE @Search OR qt.TemplateName LIKE @Search)";
            if (isActive.HasValue)
                whereClause += " AND qt.IsActive = @IsActive";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM QC.QualityTemplate qt
                WHERE {whereClause};

                SELECT
                    qt.Id, qt.TemplateCode, qt.TemplateName, qt.Description,
                    qt.IsActive, qt.IsDeleted,
                    qt.CreatedBy, qt.CreatedDate, qt.CreatedByName, qt.CreatedIP,
                    qt.ModifiedBy, qt.ModifiedDate, qt.ModifiedByName, qt.ModifiedIP,
                    ISNULL((
                        SELECT COUNT(*) FROM QC.QualityTemplateParameter qtp
                        WHERE qtp.QualityTemplateId = qt.Id AND qtp.IsDeleted = 0
                    ), 0) AS ParameterCount
                FROM QC.QualityTemplate qt
                WHERE {whereClause}
                ORDER BY qt.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                IsActive = isActive,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<QualityTemplateListDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<QualityTemplateDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    qt.Id, qt.TemplateCode, qt.TemplateName, qt.Description,
                    qt.IsActive, qt.IsDeleted,
                    qt.CreatedBy, qt.CreatedDate, qt.CreatedByName, qt.CreatedIP,
                    qt.ModifiedBy, qt.ModifiedDate, qt.ModifiedByName, qt.ModifiedIP
                FROM QC.QualityTemplate qt
                WHERE qt.Id = @Id AND qt.IsDeleted = 0;

                SELECT
                    qtp.Id, qtp.QualityParameterId,
                    qp.ParameterCode, qp.ParameterName,
                    qtp.SequenceNo, qtp.IsMandatory, qtp.IsCritical,
                    qtp.InspectionMethodId,
                    im.Code AS InspectionMethodCode,
                    im.Description AS InspectionMethodName,
                    qtp.SampleSize, qtp.SampleUomId,
                    qtp.IsGradeApplicable, qtp.Remarks, qtp.IsActive
                FROM QC.QualityTemplateParameter qtp
                LEFT JOIN QC.QualityParameter qp ON qtp.QualityParameterId = qp.Id AND qp.IsDeleted = 0
                LEFT JOIN QC.MiscMaster im ON qtp.InspectionMethodId = im.Id AND im.IsDeleted = 0
                WHERE qtp.QualityTemplateId = @Id AND qtp.IsDeleted = 0
                ORDER BY qtp.SequenceNo ASC;";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id });

            var dto = await multi.ReadFirstOrDefaultAsync<QualityTemplateDto>();
            if (dto == null)
                return null;

            dto.Parameters = (await multi.ReadAsync<QualityTemplateParameterDto>()).ToList();

            // Populate UOM names via cross-module lookup
            var uomIds = dto.Parameters
                .Where(p => p.SampleUomId.HasValue && p.SampleUomId.Value > 0)
                .Select(p => p.SampleUomId!.Value)
                .Distinct()
                .ToList();

            if (uomIds.Count > 0)
            {
                var uoms = await _uomLookup.GetByIdsAsync(uomIds);
                var uomDict = uoms.ToDictionary(u => u.Id);

                foreach (var p in dto.Parameters.Where(p => p.SampleUomId.HasValue))
                {
                    if (uomDict.TryGetValue(p.SampleUomId!.Value, out var uom))
                    {
                        p.SampleUomCode = uom.Code;
                        p.SampleUomName = uom.UOMName;
                    }
                }
            }

            return dto;
        }

        public async Task<IReadOnlyList<QualityTemplateLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, TemplateCode, TemplateName
                FROM QC.QualityTemplate
                WHERE IsActive = 1 AND IsDeleted = 0
                AND (@Term = '' OR TemplateCode LIKE '%' + @Term + '%' OR TemplateName LIKE '%' + @Term + '%')
                ORDER BY TemplateCode ASC";

            var result = await _dbConnection.QueryAsync<QualityTemplateLookupDto>(
                new CommandDefinition(sql, new { Term = term ?? string.Empty }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string templateName, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM QC.QualityTemplate
                WHERE LOWER(TemplateName) = LOWER(@Name)
                AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Name = templateName.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM QC.QualityTemplate
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> QualityParameterExistsAndActiveAsync(int qualityParameterId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM QC.QualityParameter
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = qualityParameterId });
            return count > 0;
        }

        public async Task<bool> InspectionMethodExistsAsync(int inspectionMethodId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mm.Id = @Id
                AND mm.IsActive = 1 AND mm.IsDeleted = 0
                AND mtm.IsDeleted = 0
                AND mtm.MiscTypeCode = 'QP_INSPECTION_METHOD'";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = inspectionMethodId });
            return count > 0;
        }

        public async Task<int> GetMaxTemplateCodeSequenceAsync()
        {
            // Extract numeric suffix from QT-000001 format and return the max value.
            const string sql = @"
                SELECT ISNULL(MAX(CAST(SUBSTRING(TemplateCode, 4, 10) AS INT)), 0)
                FROM QC.QualityTemplate
                WHERE TemplateCode LIKE 'QT-[0-9]%'";

            return await _dbConnection.ExecuteScalarAsync<int>(sql);
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [QC].[QualitySpecification] WHERE QualityTemplateId = @id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { id });
        }
    }
}
