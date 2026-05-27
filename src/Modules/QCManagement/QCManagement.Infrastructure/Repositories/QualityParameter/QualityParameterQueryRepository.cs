using System.Data;
using Dapper;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Application.QualityParameter.Dto;

namespace QCManagement.Infrastructure.Repositories.QualityParameter
{
    public class QualityParameterQueryRepository : IQualityParameterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public QualityParameterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<QualityParameterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? parameterGroupId = null)
        {
            var whereClause = "qp.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (qp.ParameterCode LIKE @Search OR qp.ParameterName LIKE @Search OR pg.Description LIKE @Search)";
            if (parameterGroupId.HasValue && parameterGroupId.Value > 0)
                whereClause += " AND qp.ParameterGroupId = @ParameterGroupId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM QC.QualityParameter qp
                LEFT JOIN QC.MiscMaster pg ON qp.ParameterGroupId = pg.Id AND pg.IsDeleted = 0
                WHERE {whereClause};

                SELECT
                    qp.Id, qp.ParameterCode, qp.ParameterName,
                    qp.ParameterGroupId, pg.Code AS ParameterGroupCode, pg.Description AS ParameterGroupName,
                    qp.DataTypeId, dt.Code AS DataTypeCode, dt.Description AS DataTypeName,
                    qp.UnitId,
                    qp.ValidationTypeId, vt.Code AS ValidationTypeCode, vt.Description AS ValidationTypeName,
                    qp.Description,
                    qp.IsActive, qp.IsDeleted,
                    qp.CreatedBy, qp.CreatedDate, qp.CreatedByName, qp.CreatedIP,
                    qp.ModifiedBy, qp.ModifiedDate, qp.ModifiedByName, qp.ModifiedIP
                FROM QC.QualityParameter qp
                LEFT JOIN QC.MiscMaster pg ON qp.ParameterGroupId = pg.Id AND pg.IsDeleted = 0
                LEFT JOIN QC.MiscMaster dt ON qp.DataTypeId   = dt.Id AND dt.IsDeleted = 0
                LEFT JOIN QC.MiscMaster vt ON qp.ValidationTypeId = vt.Id AND vt.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY qp.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                ParameterGroupId = parameterGroupId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<QualityParameterDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<QualityParameterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    qp.Id, qp.ParameterCode, qp.ParameterName,
                    qp.ParameterGroupId, pg.Code AS ParameterGroupCode, pg.Description AS ParameterGroupName,
                    qp.DataTypeId, dt.Code AS DataTypeCode, dt.Description AS DataTypeName,
                    qp.UnitId,
                    qp.ValidationTypeId, vt.Code AS ValidationTypeCode, vt.Description AS ValidationTypeName,
                    qp.Description,
                    qp.IsActive, qp.IsDeleted,
                    qp.CreatedBy, qp.CreatedDate, qp.CreatedByName, qp.CreatedIP,
                    qp.ModifiedBy, qp.ModifiedDate, qp.ModifiedByName, qp.ModifiedIP
                FROM QC.QualityParameter qp
                LEFT JOIN QC.MiscMaster pg ON qp.ParameterGroupId = pg.Id AND pg.IsDeleted = 0
                LEFT JOIN QC.MiscMaster dt ON qp.DataTypeId   = dt.Id AND dt.IsDeleted = 0
                LEFT JOIN QC.MiscMaster vt ON qp.ValidationTypeId = vt.Id AND vt.IsDeleted = 0
                WHERE qp.Id = @Id AND qp.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<QualityParameterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<QualityParameterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, ParameterCode, ParameterName
                FROM QC.QualityParameter
                WHERE IsDeleted = 0 AND IsActive = 1
                AND (ParameterCode LIKE @Term OR ParameterName LIKE @Term)
                ORDER BY ParameterCode ASC";

            var result = await _dbConnection.QueryAsync<QualityParameterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string parameterName, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM QC.QualityParameter
                WHERE LOWER(ParameterName) = LOWER(@Name)
                AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Name = parameterName.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM QC.QualityParameter
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> ParameterGroupExistsAsync(int parameterGroupId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mm.Id = @Id
                AND mm.IsActive = 1 AND mm.IsDeleted = 0
                AND mtm.IsDeleted = 0
                AND mtm.MiscTypeCode = 'QP_GROUP'";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = parameterGroupId });
            return count > 0;
        }

        public async Task<bool> DataTypeExistsAsync(int dataTypeId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mm.Id = @Id
                AND mm.IsActive = 1 AND mm.IsDeleted = 0
                AND mtm.IsDeleted = 0
                AND mtm.MiscTypeCode = 'QP_DATATYPE'";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = dataTypeId });
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

        public async Task<bool> IsUomRequiredForDataTypeAsync(int dataTypeId)
        {
            // UOM is mandatory only when DataType is Numeric (NUM) or Decimal (DEC).
            const string sql = @"
                SELECT COUNT(1)
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mm.Id = @Id
                AND mm.IsDeleted = 0
                AND mtm.IsDeleted = 0
                AND mtm.MiscTypeCode = 'QP_DATATYPE'
                AND mm.Code IN ('NUM', 'DEC')";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = dataTypeId });
            return count > 0;
        }

        public async Task<int?> GetDataTypeIdByQualityParameterIdAsync(int qualityParameterId)
        {
            const string sql = @"
                SELECT DataTypeId
                FROM QC.QualityParameter
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.ExecuteScalarAsync<int?>(sql, new { Id = qualityParameterId });
        }

        public Task<bool> SoftDeleteValidationAsync(int id)
        {
            // No entities depend on QualityParameter yet. Quality Template / Spec / Inspection
            // entities (when built) MUST extend this to block delete when dependents exist.
            return Task.FromResult(false);
        }
    }
}
