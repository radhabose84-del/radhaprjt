using System.Data;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.Lookups
{
    /// <summary>
    /// Returns transporters from the ERP Party Master. Per the Freight RFQ design decision, any
    /// active, non-deleted party may act as a transporter (mirroring how Sales DeliveryChallan
    /// resolves TransporterId via the generic party lookup, with no party-type filter).
    /// Results are cached globally by CachedLookupDecorator (interface name ends with "Lookup").
    /// </summary>
    internal sealed class TransporterLookupRepository : ITransporterLookup
    {
        private readonly IDbConnection _dbConnection;

        public TransporterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<TransporterLookupDto>> SearchTransportersAsync(
            string? term, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT a.Id,
                       a.PartyCode AS TransporterCode,
                       a.PartyName AS TransporterName
                FROM Party.PartyMaster a
                WHERE a.IsActive = 1
                  AND a.IsDeleted = 0
                  AND (@Term IS NULL OR a.PartyName LIKE @Pattern OR a.PartyCode LIKE @Pattern)
                ORDER BY a.PartyName ASC;";

            var trimmed = string.IsNullOrWhiteSpace(term) ? null : term.Trim();
            var result = await _dbConnection.QueryAsync<TransporterLookupDto>(
                new CommandDefinition(
                    sql,
                    new { Term = trimmed, Pattern = $"%{trimmed}%" },
                    cancellationToken: ct));

            return result.ToList();
        }

        public async Task<TransporterLookupDto?> GetActiveTransporterByIdAsync(
            int partyId, CancellationToken ct = default)
        {
            if (partyId <= 0)
                return null;

            const string sql = @"
                SELECT TOP 1
                       a.Id,
                       a.PartyCode AS TransporterCode,
                       a.PartyName AS TransporterName
                FROM Party.PartyMaster a
                WHERE a.Id = @PartyId
                  AND a.IsActive = 1
                  AND a.IsDeleted = 0;";

            return await _dbConnection.QueryFirstOrDefaultAsync<TransporterLookupDto>(
                new CommandDefinition(sql, new { PartyId = partyId }, cancellationToken: ct));
        }
    }
}
