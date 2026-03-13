using System.Data;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.Lookups
{
    internal sealed class AgentLookupRepository : IAgentLookup
    {
        private readonly IDbConnection _dbConnection;

        public AgentLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<AgentLookupDto>> GetAllAgentAsync()
        {
            const string sql = @"
                SELECT DISTINCT p.Id, p.PartyCode AS AgentCode, p.PartyName AS AgentName
                FROM Party.PartyMaster p
                INNER JOIN Party.PartyType pt ON pt.PartyId = p.Id
                INNER JOIN Party.MiscMaster mm ON pt.PartyTypeId = mm.Id
                WHERE mm.Description = 'AGENT'
                  AND p.IsActive = 1
                  AND p.IsDeleted = 0
                ORDER BY p.PartyName ASC;";

            var result = await _dbConnection.QueryAsync<AgentLookupDto>(sql);
            return result.ToList();
        }
    }
}
