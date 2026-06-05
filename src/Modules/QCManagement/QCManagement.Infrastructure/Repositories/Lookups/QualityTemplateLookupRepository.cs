using System.Data;
using Contracts.Dtos.Lookups.QC;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.QC;
using Dapper;

namespace QCManagement.Infrastructure.Repositories.Lookups
{
    /// <summary>
    /// Cross-module lookup over the QC quality-template masters. Reads only active,
    /// non-deleted rows and resolves the parameter UOM name via the Inventory UOM lookup
    /// (the same approach used by QualityTemplateQueryRepository).
    /// </summary>
    internal sealed class QualityTemplateLookupRepository : IQualityTemplateLookup
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUOMLookup _uomLookup;

        public QualityTemplateLookupRepository(IDbConnection dbConnection, IUOMLookup uomLookup)
        {
            _dbConnection = dbConnection;
            _uomLookup = uomLookup;
        }

        public async Task<IReadOnlyList<QualityTemplateLookupDto>> GetByIdsAsync(
            IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Where(i => i > 0).Distinct().ToList() ?? new List<int>();
            if (idList.Count == 0)
                return new List<QualityTemplateLookupDto>();

            const string sql = @"
                SELECT Id, TemplateCode, TemplateName
                FROM QC.QualityTemplate
                WHERE Id IN @Ids AND IsDeleted = 0;";

            var rows = await _dbConnection.QueryAsync<QualityTemplateLookupDto>(
                new CommandDefinition(sql, new { Ids = idList }, cancellationToken: ct));
            return rows.ToList();
        }

        public async Task<IReadOnlyList<QualityTemplateParameterLookupDto>> GetParametersByTemplateIdAsync(
            int qualityTemplateId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    qp.Id            AS QualityParameterId,
                    qp.ParameterCode AS ParameterCode,
                    qp.ParameterName AS ParameterName,
                    qtp.SequenceNo   AS SequenceNo,
                    qtp.IsMandatory  AS IsMandatory,
                    qp.UnitId        AS UnitId
                FROM QC.QualityTemplateParameter qtp
                INNER JOIN QC.QualityParameter qp
                    ON qtp.QualityParameterId = qp.Id AND qp.IsActive = 1 AND qp.IsDeleted = 0
                WHERE qtp.QualityTemplateId = @TemplateId
                  AND qtp.IsActive = 1 AND qtp.IsDeleted = 0
                ORDER BY qtp.SequenceNo ASC;";

            var rows = (await _dbConnection.QueryAsync<QualityTemplateParameterLookupDto>(
                new CommandDefinition(sql, new { TemplateId = qualityTemplateId }, cancellationToken: ct))).ToList();

            await PopulateUnitNamesAsync(rows, ct);
            return rows;
        }

        public async Task<IReadOnlyList<QualityTemplateParameterLookupDto>> GetParametersByIdsAsync(
            IEnumerable<int> qualityParameterIds, CancellationToken ct = default)
        {
            var idList = qualityParameterIds?.Where(i => i > 0).Distinct().ToList() ?? new List<int>();
            if (idList.Count == 0)
                return new List<QualityTemplateParameterLookupDto>();

            const string sql = @"
                SELECT
                    qp.Id            AS QualityParameterId,
                    qp.ParameterCode AS ParameterCode,
                    qp.ParameterName AS ParameterName,
                    0                AS SequenceNo,
                    CAST(0 AS BIT)   AS IsMandatory,
                    qp.UnitId        AS UnitId
                FROM QC.QualityParameter qp
                WHERE qp.Id IN @Ids AND qp.IsDeleted = 0;";

            var rows = (await _dbConnection.QueryAsync<QualityTemplateParameterLookupDto>(
                new CommandDefinition(sql, new { Ids = idList }, cancellationToken: ct))).ToList();

            await PopulateUnitNamesAsync(rows, ct);
            return rows;
        }

        private async Task PopulateUnitNamesAsync(List<QualityTemplateParameterLookupDto> rows, CancellationToken ct)
        {
            var unitIds = rows
                .Where(r => r.UnitId.HasValue && r.UnitId.Value > 0)
                .Select(r => r.UnitId!.Value)
                .Distinct()
                .ToList();

            if (unitIds.Count == 0)
                return;

            var uoms = await _uomLookup.GetByIdsAsync(unitIds, ct);
            var uomDict = uoms.ToDictionary(u => u.Id);

            foreach (var r in rows.Where(r => r.UnitId.HasValue))
            {
                if (uomDict.TryGetValue(r.UnitId!.Value, out var uom))
                    r.UnitName = uom.UOMName;
            }
        }
    }
}
