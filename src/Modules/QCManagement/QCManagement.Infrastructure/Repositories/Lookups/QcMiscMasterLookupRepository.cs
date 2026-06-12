using System.Data;
using Contracts.Dtos.Lookups.QC;
using Contracts.Interfaces.Lookups.QC;
using Dapper;

namespace QCManagement.Infrastructure.Repositories.Lookups
{
    /// <summary>
    /// Resolves QC.MiscMaster rows by Id for cross-module consumers (e.g. Purchase Arrival/GRN
    /// resolving QcStatusId → status name). Non-deleted rows only — the name must still resolve
    /// for a status that has since been inactivated.
    /// </summary>
    internal sealed class QcMiscMasterLookupRepository : IQcMiscMasterLookup
    {
        private readonly IDbConnection _dbConnection;

        public QcMiscMasterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<QcMiscMasterLookupDto>> GetByIdsAsync(
            IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Where(i => i > 0).Distinct().ToList() ?? new List<int>();
            if (idList.Count == 0)
                return new List<QcMiscMasterLookupDto>();

            const string sql = @"
                SELECT Id, Code, Description
                FROM QC.MiscMaster
                WHERE Id IN @Ids AND IsDeleted = 0;";

            var rows = await _dbConnection.QueryAsync<QcMiscMasterLookupDto>(
                new CommandDefinition(sql, new { Ids = idList }, cancellationToken: ct));
            return rows.ToList();
        }

        public async Task<int?> GetIdByTypeAndCodeAsync(string miscTypeCode, string code, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(miscTypeCode) || string.IsNullOrWhiteSpace(code))
                return null;

            const string sql = @"
                SELECT TOP 1 mm.Id
                FROM QC.MiscMaster mm
                INNER JOIN QC.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id
                WHERE mtm.MiscTypeCode = @MiscTypeCode
                  AND mm.Code = @Code
                  AND mm.IsActive = 1 AND mm.IsDeleted = 0
                ORDER BY mm.Id ASC;";

            return await _dbConnection.ExecuteScalarAsync<int?>(
                new CommandDefinition(sql, new { MiscTypeCode = miscTypeCode, Code = code }, cancellationToken: ct));
        }
    }
}
