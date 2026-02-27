using System.Data;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Dto;

namespace SalesManagement.Infrastructure.Repositories.OfficerAgent
{
    public class OfficerAgentQueryRepository : IOfficerAgentQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;

        public OfficerAgentQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
        }

        public async Task<(List<OfficerAgentDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (mo.EmployeeName LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.OfficerAgent oa
                LEFT JOIN Sales.MarketingOfficer mo ON oa.MarketingOfficerId = mo.Id AND mo.IsDeleted = 0
                WHERE 1=1
                {searchFilter};

                SELECT
                    oa.Id, oa.AgentId, oa.MarketingOfficerId,
                    oa.ValidityFrom, oa.ValidityTo, oa.IsActive,
                    oa.CreatedBy, oa.CreatedDate, oa.CreatedByName, oa.CreatedIP,
                    oa.ModifiedBy, oa.ModifiedDate, oa.ModifiedByName, oa.ModifiedIP,
                    mo.EmployeeName AS OfficerName
                FROM Sales.OfficerAgent oa
                LEFT JOIN Sales.MarketingOfficer mo ON oa.MarketingOfficerId = mo.Id AND mo.IsDeleted = 0
                WHERE 1=1
                {searchFilter}
                ORDER BY oa.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<OfficerAgentDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                var agentIds = list.Select(x => x.AgentId).Distinct();
                var agents = await _partyLookup.GetByIdsAsync(agentIds);
                var agentDict = agents.ToDictionary(x => x.Id);

                foreach (var item in list)
                {
                    if (agentDict.TryGetValue(item.AgentId, out var agentData))
                        item.AgentName = agentData.PartyName;
                }
            }

            return (list, totalCount);
        }

        public async Task<OfficerAgentDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    oa.Id, oa.AgentId, oa.MarketingOfficerId,
                    oa.ValidityFrom, oa.ValidityTo, oa.IsActive,
                    oa.CreatedBy, oa.CreatedDate, oa.CreatedByName, oa.CreatedIP,
                    oa.ModifiedBy, oa.ModifiedDate, oa.ModifiedByName, oa.ModifiedIP,
                    mo.EmployeeName AS OfficerName
                FROM Sales.OfficerAgent oa
                LEFT JOIN Sales.MarketingOfficer mo ON oa.MarketingOfficerId = mo.Id AND mo.IsDeleted = 0
                WHERE oa.Id = @Id";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<OfficerAgentDto>(sql, new { Id = id });

            if (dto != null)
            {
                var agents = await _partyLookup.GetByIdsAsync(new[] { dto.AgentId });
                var agentData = agents.FirstOrDefault();
                if (agentData != null)
                    dto.AgentName = agentData.PartyName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<OfficerAgentLookupDto>> AutocompleteAsync(
            string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20
                    oa.Id, oa.AgentId,
                    mo.EmployeeName AS OfficerName
                FROM Sales.OfficerAgent oa
                LEFT JOIN Sales.MarketingOfficer mo ON oa.MarketingOfficerId = mo.Id AND mo.IsDeleted = 0
                WHERE oa.IsActive = 1
                  AND (mo.EmployeeName LIKE @Term)
                ORDER BY oa.Id DESC";

            var rows = (await _dbConnection.QueryAsync<dynamic>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct)))
                .ToList();

            if (rows.Count == 0)
                return new List<OfficerAgentLookupDto>();

            var agentIds = rows.Select(r => (int)r.AgentId).Distinct();
            var agents = await _partyLookup.GetByIdsAsync(agentIds, ct);
            var agentDict = agents.ToDictionary(x => x.Id);

            return rows.Select(r =>
            {
                agentDict.TryGetValue((int)r.AgentId, out var agentData);
                return new OfficerAgentLookupDto
                {
                    Id = (int)r.Id,
                    AgentId = (int)r.AgentId,
                    AgentName = agentData?.PartyName,
                    OfficerName = (string?)r.OfficerName
                };
            }).ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.OfficerAgent WHERE Id = @Id";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> AgentExistsAsync(int agentId, CancellationToken ct = default)
        {
            var agents = await _partyLookup.GetByIdsAsync(new[] { agentId }, ct);
            return agents.Any();
        }

        public async Task<bool> MarketingOfficerExistsAsync(int officerId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.MarketingOfficer
                WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = officerId });
            return count > 0;
        }

        public async Task<bool> IsExpiredAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.OfficerAgent
                WHERE Id = @Id AND ValidityTo < CAST(GETDATE() AS DATE)";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }
    }
}
