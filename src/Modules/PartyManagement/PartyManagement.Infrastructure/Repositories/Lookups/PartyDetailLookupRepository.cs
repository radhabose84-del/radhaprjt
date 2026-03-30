using System.Data;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.Lookups
{
    internal sealed class PartyDetailLookupRepository : IPartyDetailLookup
    {
        private readonly IDbConnection _dbConnection;

        public PartyDetailLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<PartyDetailLookupDto?> GetByIdAsync(int partyId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT pm.Id, pm.PartyCode, pm.PartyName,
                    pm.GSTNumber, pm.PAN, pm.GSTStateCode,
                    pm.CreditDays, pm.IsGstReverseCharge,
                    pa.AddressLine1, pa.AddressLine2,
                    pa.CityId, pa.StateId, pa.PostalCode,
                    pc.MobileNo, pc.Phone
                FROM Party.PartyMaster pm
                LEFT JOIN Party.PartyAddress pa ON pa.PartyId = pm.Id
                LEFT JOIN Party.PartyContact pc ON pc.PartyId = pm.Id
                WHERE pm.Id = @PartyId";

            return await _dbConnection.QueryFirstOrDefaultAsync<PartyDetailLookupDto>(
                new CommandDefinition(sql, new { PartyId = partyId }, cancellationToken: ct));
        }
    }
}
