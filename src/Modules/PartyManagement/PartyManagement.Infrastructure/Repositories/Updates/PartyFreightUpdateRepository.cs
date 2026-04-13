using System.Data;
using Contracts.Interfaces.Updates.Party;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.Updates
{
    internal sealed class PartyFreightUpdateRepository : IPartyFreightUpdate
    {
        public async Task UpdateSalesFreightIfNullAsync(int partyId, int freightId, IDbConnection connection, IDbTransaction transaction)
        {
            const string sql = @"
                UPDATE Party.PartyMaster
                SET SalesFreightId = @FreightId
                WHERE Id = @PartyId AND SalesFreightId IS NULL AND IsDeleted = 0";

            await connection.ExecuteAsync(sql, new { FreightId = freightId, PartyId = partyId }, transaction);
        }
    }
}
