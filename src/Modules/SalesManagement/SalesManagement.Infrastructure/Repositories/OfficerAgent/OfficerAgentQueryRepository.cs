using System.Data;
using Contracts.Interfaces;
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
        private readonly IIPAddressService _ipAddressService;

        public OfficerAgentQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<OfficerAgentGroupedDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND mo.EmployeeName LIKE @Search";

            // Marketing Officer access scoping: when EmpId is set, restrict to own record only
            var empId = _ipAddressService.GetEmpId();
            var moFilter = empId.HasValue ? " AND mo.Id = @EmpId " : "";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.MarketingOfficer mo
                WHERE mo.IsDeleted = 0
                {searchFilter}
                {moFilter};

                SELECT
                    mo.Id AS MarketingOfficerId, mo.EmployeeNo,
                    mo.EmployeeName AS OfficerName, mo.Designation,
                    mo.MobileNo, mo.Email, mo.Unit, mo.Department,
                    mo.SalesOfficeId, so.SalesOfficeName
                FROM Sales.MarketingOfficer mo
                LEFT JOIN Sales.SalesOffice so ON mo.SalesOfficeId = so.Id AND so.IsDeleted = 0
                WHERE mo.IsDeleted = 0
                {searchFilter}
                {moFilter}
                ORDER BY mo.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT
                    oa.Id AS AssignmentId, oa.AgentId, oa.MarketingOfficerId,
                    oa.ValidityFrom, oa.ValidityTo, oa.IsActive,
                    oa.CreatedBy, oa.CreatedDate, oa.CreatedByName, oa.CreatedIP,
                    oa.ModifiedBy, oa.ModifiedDate, oa.ModifiedByName, oa.ModifiedIP
                FROM Sales.OfficerAgent oa
                WHERE oa.MarketingOfficerId IN (
                    SELECT Id FROM Sales.MarketingOfficer mo
                    WHERE IsDeleted = 0
                    {searchFilter}
                    {moFilter}
                    ORDER BY Id DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                )
                ORDER BY oa.MarketingOfficerId, oa.Id DESC;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new DynamicParameters();
            parameters.Add("Search", $"%{searchTerm}%");
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("PageSize", pageSize);
            if (empId.HasValue)
                parameters.Add("EmpId", empId.Value);

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var officers = (await multi.ReadAsync<OfficerAgentGroupedDto>()).ToList();
            var assignmentRows = (await multi.ReadAsync<AssignmentRow>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (officers.Count > 0 && assignmentRows.Count > 0)
            {
                var agentIds = assignmentRows.Select(x => x.AgentId).Distinct();
                var agents = await _partyLookup.GetByIdsAsync(agentIds);
                var agentDict = agents.ToDictionary(x => x.Id);
                var officerDict = officers.ToDictionary(o => o.MarketingOfficerId);

                foreach (var row in assignmentRows)
                {
                    agentDict.TryGetValue(row.AgentId, out var agentData);

                    var item = new OfficerAgentItemDto
                    {
                        AssignmentId = row.AssignmentId,
                        AgentId = row.AgentId,
                        AgentName = agentData?.PartyName,
                        AgentMobile = agentData?.Mobile,
                        ValidityFrom = row.ValidityFrom,
                        ValidityTo = row.ValidityTo,
                        IsActive = row.IsActive,
                        CreatedBy = row.CreatedBy,
                        CreatedDate = row.CreatedDate,
                        CreatedByName = row.CreatedByName,
                        CreatedIP = row.CreatedIP,
                        ModifiedBy = row.ModifiedBy,
                        ModifiedDate = row.ModifiedDate,
                        ModifiedByName = row.ModifiedByName,
                        ModifiedIP = row.ModifiedIP
                    };

                    if (officerDict.TryGetValue(row.MarketingOfficerId, out var officer))
                        officer.Agents.Add(item);
                }
            }

            return (officers, totalCount);
        }

        public async Task<OfficerAgentGroupedDto?> GetByIdAsync(int marketingOfficerId)
        {
            // Marketing Officer access scoping: officer can only fetch own record
            var empId = _ipAddressService.GetEmpId();
            if (empId.HasValue && empId.Value != marketingOfficerId)
                return null;

            const string sql = @"
                SELECT
                    mo.Id AS MarketingOfficerId, mo.EmployeeNo,
                    mo.EmployeeName AS OfficerName, mo.Designation,
                    mo.MobileNo, mo.Email, mo.Unit, mo.Department,
                    mo.SalesOfficeId, so.SalesOfficeName
                FROM Sales.MarketingOfficer mo
                LEFT JOIN Sales.SalesOffice so ON mo.SalesOfficeId = so.Id AND so.IsDeleted = 0
                WHERE mo.Id = @Id AND mo.IsDeleted = 0;

                SELECT
                    oa.Id AS AssignmentId, oa.AgentId,
                    oa.ValidityFrom, oa.ValidityTo, oa.IsActive,
                    oa.CreatedBy, oa.CreatedDate, oa.CreatedByName, oa.CreatedIP,
                    oa.ModifiedBy, oa.ModifiedDate, oa.ModifiedByName, oa.ModifiedIP
                FROM Sales.OfficerAgent oa
                WHERE oa.MarketingOfficerId = @Id
                ORDER BY oa.Id DESC;";

            var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = marketingOfficerId });

            var grouped = await multi.ReadFirstOrDefaultAsync<OfficerAgentGroupedDto>();
            if (grouped == null)
                return null;

            var items = (await multi.ReadAsync<OfficerAgentItemDto>()).ToList();

            if (items.Count > 0)
            {
                var agentIds = items.Select(x => x.AgentId).Distinct();
                var agents = await _partyLookup.GetByIdsAsync(agentIds);
                var agentDict = agents.ToDictionary(x => x.Id);

                foreach (var item in items)
                {
                    if (agentDict.TryGetValue(item.AgentId, out var agentData))
                    {
                        item.AgentName = agentData.PartyName;
                        item.AgentMobile = agentData.Mobile;
                    }
                }
            }

            grouped.Agents = items;
            return grouped;
        }

        public async Task<IReadOnlyList<OfficerAgentGroupedDto>> AutocompleteAsync(
            string term, CancellationToken ct)
        {
            // Marketing Officer access scoping: restrict to own record only
            var empId = _ipAddressService.GetEmpId();
            var moFilter = empId.HasValue ? " AND Id = @EmpId " : "";

            var sql = $@"
                SELECT
                    mo.Id AS MarketingOfficerId, mo.EmployeeNo,
                    mo.EmployeeName AS OfficerName, mo.Designation,
                    mo.MobileNo, mo.Email, mo.Unit, mo.Department,
                    mo.SalesOfficeId, so.SalesOfficeName
                FROM (
                    SELECT TOP 20 * FROM Sales.MarketingOfficer
                    WHERE IsDeleted = 0 AND IsActive = 1 AND EmployeeName LIKE @Term
                    {moFilter}
                    ORDER BY EmployeeName
                ) mo
                LEFT JOIN Sales.SalesOffice so ON mo.SalesOfficeId = so.Id AND so.IsDeleted = 0;

                SELECT
                    oa.Id AS AssignmentId, oa.AgentId, oa.MarketingOfficerId,
                    oa.ValidityFrom, oa.ValidityTo, oa.IsActive,
                    oa.CreatedBy, oa.CreatedDate, oa.CreatedByName, oa.CreatedIP,
                    oa.ModifiedBy, oa.ModifiedDate, oa.ModifiedByName, oa.ModifiedIP
                FROM Sales.OfficerAgent oa
                WHERE oa.MarketingOfficerId IN (
                    SELECT TOP 20 Id FROM Sales.MarketingOfficer
                    WHERE IsDeleted = 0 AND IsActive = 1 AND EmployeeName LIKE @Term
                    {moFilter}
                    ORDER BY EmployeeName
                )
                ORDER BY oa.MarketingOfficerId, oa.Id DESC;";

            var parameters = new DynamicParameters();
            parameters.Add("Term", $"%{term}%");
            if (empId.HasValue)
                parameters.Add("EmpId", empId.Value);

            var multi = await _dbConnection.QueryMultipleAsync(
                new CommandDefinition(sql, parameters, cancellationToken: ct));

            var officers = (await multi.ReadAsync<OfficerAgentGroupedDto>()).ToList();
            var assignmentRows = (await multi.ReadAsync<AssignmentRow>()).ToList();

            if (officers.Count == 0)
                return new List<OfficerAgentGroupedDto>();

            if (assignmentRows.Count > 0)
            {
                var agentIds = assignmentRows.Select(x => x.AgentId).Distinct();
                var agents = await _partyLookup.GetByIdsAsync(agentIds, ct);
                var agentDict = agents.ToDictionary(x => x.Id);
                var officerDict = officers.ToDictionary(o => o.MarketingOfficerId);

                foreach (var row in assignmentRows)
                {
                    agentDict.TryGetValue(row.AgentId, out var agentData);

                    var item = new OfficerAgentItemDto
                    {
                        AssignmentId = row.AssignmentId,
                        AgentId = row.AgentId,
                        AgentName = agentData?.PartyName,
                        AgentMobile = agentData?.Mobile,
                        ValidityFrom = row.ValidityFrom,
                        ValidityTo = row.ValidityTo,
                        IsActive = row.IsActive,
                        CreatedBy = row.CreatedBy,
                        CreatedDate = row.CreatedDate,
                        CreatedByName = row.CreatedByName,
                        CreatedIP = row.CreatedIP,
                        ModifiedBy = row.ModifiedBy,
                        ModifiedDate = row.ModifiedDate,
                        ModifiedByName = row.ModifiedByName,
                        ModifiedIP = row.ModifiedIP
                    };

                    if (officerDict.TryGetValue(row.MarketingOfficerId, out var officer))
                        officer.Agents.Add(item);
                }
            }

            return officers;
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

        // Private helper — holds MarketingOfficerId for grouping
        private sealed class AssignmentRow : OfficerAgentItemDto
        {
            public int MarketingOfficerId { get; set; }
        }
    }
}
