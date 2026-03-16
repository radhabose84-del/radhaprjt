using System.Data;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.Lookups;

internal sealed class SubAgentLookupRepository : ISubAgentLookup
{
    private readonly IDbConnection _dbConnection;

    public SubAgentLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IReadOnlyList<SubAgentLookupDto>> GetAllSubAgentAsync()
    {
        const string sql = @"
            SELECT DISTINCT p.Id, p.PartyCode AS SubAgentCode, p.PartyName AS SubAgentName
            FROM Party.PartyMaster p
            INNER JOIN Party.PartyType pt ON pt.PartyId = p.Id
            INNER JOIN Party.MiscMaster mm ON pt.PartyTypeId = mm.Id
            WHERE mm.Description = 'SUBAGENT'
              AND p.IsActive = 1
              AND p.IsDeleted = 0
            ORDER BY p.PartyName ASC;";

        var result = await _dbConnection.QueryAsync<SubAgentLookupDto>(sql);
        return result.ToList();
    }
}
