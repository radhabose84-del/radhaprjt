using System.Data;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.Lookups
{
    internal sealed class PartyLookupRepository : IPartyLookup
    {
        private readonly IDbConnection _dbConnection;

        public PartyLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<PartyLookupDto?> GetByIdAsync(int partyId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT p.Id, p.PartyCode, p.PartyName, c.Email, c.Mobile
                FROM Party.PartyMaster p
                OUTER APPLY (
                    SELECT TOP 1
                        pc.EmailID AS Email,
                        pc.MobileNo AS Mobile
                    FROM Party.PartyContact pc
                    WHERE pc.PartyId = p.Id
                      AND pc.MobileNo IS NOT NULL
                      AND pc.MobileNo <> ''
                    ORDER BY pc.Id
                ) c
                WHERE p.Id = @PartyId;";

            return await _dbConnection.QueryFirstOrDefaultAsync<PartyLookupDto>(
                new CommandDefinition(sql, new { PartyId = partyId }, cancellationToken: ct));
        }

        public async Task<IReadOnlyList<PartyLookupDto>> GetByIdsAsync(IEnumerable<int> partyIds, CancellationToken ct = default)
        {
            var ids = partyIds?.ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<PartyLookupDto>();

            const string sql = @"
                SELECT p.Id, p.PartyCode, p.PartyName, c.Email, c.Mobile
                FROM Party.PartyMaster p
                OUTER APPLY (
                    SELECT TOP 1
                        pc.EmailID AS Email,
                        pc.MobileNo AS Mobile
                    FROM Party.PartyContact pc
                    WHERE pc.PartyId = p.Id
                      AND pc.MobileNo IS NOT NULL
                      AND pc.MobileNo <> ''
                    ORDER BY pc.Id
                ) c
                WHERE p.Id IN @PartyIds;";

            var result = await _dbConnection.QueryAsync<PartyLookupDto>(
                new CommandDefinition(sql, new { PartyIds = ids }, cancellationToken: ct));

            return result.ToList();
        }
    }
}
