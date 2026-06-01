using System.Data;
using Contracts.Dtos.Lookups.Gate;
using Contracts.Interfaces.Lookups.Gate;
using Dapper;

namespace GateEntryManagement.Infrastructure.Repositories.Lookups
{
    /// <summary>
    /// Reads <c>Gate.GateInwardHdr</c> rows by id, returning the minimal display fields
    /// (<c>GateEntryNo</c>, <c>CreatedDate</c> as <c>GateEntryDate</c>) that other modules
    /// need without a cross-schema JOIN. Soft-deleted rows are excluded.
    /// </summary>
    internal sealed class GateInwardLookupRepository : IGateInwardLookup
    {
        private readonly IDbConnection _dbConnection;

        public GateInwardLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<GateInwardLookupDto>> GetByIdsAsync(
            IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
            if (idList.Count == 0)
                return Array.Empty<GateInwardLookupDto>();

            const string sql = @"
                SELECT
                    Id,
                    GateEntryNo,
                    CreatedDate AS GateEntryDate
                FROM Gate.GateInwardHdr
                WHERE Id IN @Ids
                  AND IsDeleted = 0;";

            var rows = await _dbConnection.QueryAsync<GateInwardLookupDto>(
                new CommandDefinition(sql, new { Ids = idList }, cancellationToken: ct));
            return rows.ToList();
        }
    }
}
