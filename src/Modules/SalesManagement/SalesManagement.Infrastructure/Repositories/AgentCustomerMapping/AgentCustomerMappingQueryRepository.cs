using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using SalesManagement.Application.AgentCustomerMapping.Dto;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;

namespace SalesManagement.Infrastructure.Repositories.AgentCustomerMapping
{
    public class AgentCustomerMappingQueryRepository : IAgentCustomerMappingQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICustomerLookup _customerLookup;
        private readonly IAgentLookup _agentLookup;
        private readonly ISubAgentLookup _subAgentLookup;
        private readonly IDataAccessFilter _dataAccessFilter;

        public AgentCustomerMappingQueryRepository(
            IDbConnection dbConnection,
            ICustomerLookup customerLookup,
            IAgentLookup agentLookup,
            ISubAgentLookup subAgentLookup,
            IDataAccessFilter dataAccessFilter)
        {
            _dbConnection = dbConnection;
            _customerLookup = customerLookup;
            _agentLookup = agentLookup;
            _subAgentLookup = subAgentLookup;
            _dataAccessFilter = dataAccessFilter;
        }

        public async Task<(List<AgentCustomerMappingDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            // Data access control
            var accessCtx = await _dataAccessFilter.GetContextAsync();

            // Agent: only their own mappings
            // Marketing Officer: only their assigned agents' mappings
            // Admin (bypass): all mappings
            var accessFilter = string.Empty;
            var dp = new DynamicParameters();
            dp.Add("Search", $"%{searchTerm}%");
            dp.Add("Offset", (pageNumber - 1) * pageSize);
            dp.Add("PageSize", pageSize);

            if (!accessCtx.BypassDataAccess)
            {
                if (accessCtx.PartyId.HasValue)
                {
                    // Agent → only their own mappings
                    accessFilter = " AND acm.AgentId = @PartyId";
                    dp.Add("PartyId", accessCtx.PartyId.Value);
                }
                else if (accessCtx.AllowedAgentIds.Count > 0)
                {
                    // Marketing Officer → their assigned agents' mappings
                    accessFilter = " AND acm.AgentId IN @AllowedAgentIds";
                    dp.Add("AllowedAgentIds", accessCtx.AllowedAgentIds.ToList());
                }
                else if (accessCtx.IsCustomerRestricted)
                {
                    // Restricted but no agents → return empty
                    return (new List<AgentCustomerMappingDto>(), 0);
                }
            }

            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (acm.Remarks LIKE @Search)";

            var allFilters = $"{searchFilter}{accessFilter}";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.AgentCustomerMapping acm
                WHERE acm.IsDeleted = 0
                {allFilters};

                SELECT
                    acm.Id, acm.CustomerId, acm.AgentId, acm.SubAgentId,
                    acm.SalesSegmentId, ss.SegmentName,
                    acm.EffectiveFrom, acm.EffectiveTo, acm.IsDefaultAgent, acm.Remarks,
                    acm.IsActive, acm.IsDeleted,
                    acm.CreatedBy, acm.CreatedDate, acm.CreatedByName, acm.CreatedIP,
                    acm.ModifiedBy, acm.ModifiedDate, acm.ModifiedByName, acm.ModifiedIP
                FROM Sales.AgentCustomerMapping acm
                LEFT JOIN Sales.SalesSegment ss ON acm.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                WHERE acm.IsDeleted = 0
                {allFilters}
                ORDER BY acm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var multi = await _dbConnection.QueryMultipleAsync(query, dp);
            var list = (await multi.ReadAsync<AgentCustomerMappingDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Any())
            {
                var allCustomers = await _customerLookup.GetAllCustomerAsync();
                var customerDict = allCustomers.ToDictionary(x => x.Id, x => x.CustomerName);

                var allAgents = await _agentLookup.GetAllAgentAsync();
                var agentDict = allAgents.ToDictionary(x => x.Id, x => x.AgentName);

                foreach (var item in list)
                {
                    if (customerDict.TryGetValue(item.CustomerId, out var customerName))
                        item.CustomerName = customerName;

                    if (agentDict.TryGetValue(item.AgentId, out var agentName))
                        item.AgentName = agentName;

                    if (item.SubAgentId.HasValue && agentDict.TryGetValue(item.SubAgentId.Value, out var subAgentName))
                        item.SubAgentName = subAgentName;
                }
            }

            return (list, totalCount);
        }

        private async Task<(string Filter, DynamicParameters Dp)> BuildAccessFilterAsync(DynamicParameters? dp = null)
        {
            dp ??= new DynamicParameters();
            var accessCtx = await _dataAccessFilter.GetContextAsync();
            var filter = string.Empty;

            if (!accessCtx.BypassDataAccess)
            {
                if (accessCtx.PartyId.HasValue)
                {
                    filter = " AND acm.AgentId = @PartyId";
                    dp.Add("PartyId", accessCtx.PartyId.Value);
                }
                else if (accessCtx.AllowedAgentIds.Count > 0)
                {
                    filter = " AND acm.AgentId IN @AllowedAgentIds";
                    dp.Add("AllowedAgentIds", accessCtx.AllowedAgentIds.ToList());
                }
                else if (accessCtx.IsCustomerRestricted)
                {
                    filter = " AND 1 = 0"; // no access → return nothing
                }
            }

            return (filter, dp);
        }

        public async Task<AgentCustomerMappingDto?> GetByIdAsync(int id)
        {
            var dp = new DynamicParameters();
            dp.Add("Id", id);
            var (accessFilter, _) = await BuildAccessFilterAsync(dp);

            var sql = $@"
                SELECT
                    acm.Id, acm.CustomerId, acm.AgentId, acm.SubAgentId,
                    acm.SalesSegmentId, ss.SegmentName,
                    acm.EffectiveFrom, acm.EffectiveTo, acm.IsDefaultAgent, acm.Remarks,
                    acm.IsActive, acm.IsDeleted,
                    acm.CreatedBy, acm.CreatedDate, acm.CreatedByName, acm.CreatedIP,
                    acm.ModifiedBy, acm.ModifiedDate, acm.ModifiedByName, acm.ModifiedIP
                FROM Sales.AgentCustomerMapping acm
                LEFT JOIN Sales.SalesSegment ss ON acm.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                WHERE acm.Id = @Id AND acm.IsDeleted = 0
                {accessFilter}";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<AgentCustomerMappingDto>(
                sql, dp);

            if (dto != null)
            {
                var allCustomers = await _customerLookup.GetAllCustomerAsync();
                var customer = allCustomers.FirstOrDefault(x => x.Id == dto.CustomerId);
                dto.CustomerName = customer?.CustomerName;

                var allAgents = await _agentLookup.GetAllAgentAsync();
                var agentDict = allAgents.ToDictionary(x => x.Id, x => x.AgentName);

                if (agentDict.TryGetValue(dto.AgentId, out var agentName))
                    dto.AgentName = agentName;

                if (dto.SubAgentId.HasValue)
                {
                    if (agentDict.TryGetValue(dto.SubAgentId.Value, out var subAgentName))
                        dto.SubAgentName = subAgentName;
                }
            }

            return dto;
        }

        public async Task<IReadOnlyList<AgentCustomerMappingLookupDto>> AutocompleteAsync(
            string term, CancellationToken ct)
        {
            var (accessFilter, dp) = await BuildAccessFilterAsync();

            var sql = $@"
                SELECT TOP 20
                    acm.Id, acm.CustomerId, acm.AgentId
                FROM Sales.AgentCustomerMapping acm
                WHERE acm.IsDeleted = 0 AND acm.IsActive = 1
                {accessFilter}
                ORDER BY acm.Id DESC";

            var rows = (await _dbConnection.QueryAsync<dynamic>(
                new CommandDefinition(sql, dp, cancellationToken: ct)))
                .ToList();

            if (!rows.Any())
                return new List<AgentCustomerMappingLookupDto>();

            var allCustomers = await _customerLookup.GetAllCustomerAsync();
            var customerDict = allCustomers.ToDictionary(x => x.Id, x => x.CustomerName);

            var allAgents = await _agentLookup.GetAllAgentAsync();
            var agentDict = allAgents.ToDictionary(x => x.Id, x => x.AgentName);

            return rows
                .Where(r => string.IsNullOrWhiteSpace(term)
                    || (customerDict.TryGetValue((int)r.CustomerId, out var cName) && cName != null && cName.Contains(term, StringComparison.OrdinalIgnoreCase))
                    || (agentDict.TryGetValue((int)r.AgentId, out var aName) && aName != null && aName.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .Select(r =>
                {
                    customerDict.TryGetValue((int)r.CustomerId, out var customerName);
                    agentDict.TryGetValue((int)r.AgentId, out var agentName);
                    return new AgentCustomerMappingLookupDto
                    {
                        Id = (int)r.Id,
                        CustomerName = customerName,
                        AgentName = agentName
                    };
                })
                .ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.AgentCustomerMapping
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> CustomerExistsAsync(int customerId, CancellationToken ct = default)
        {
            var customers = await _customerLookup.GetAllCustomerAsync();
            return customers.Any(x => x.Id == customerId);
        }

        public async Task<bool> AgentExistsAsync(int agentId, CancellationToken ct = default)
        {
            var agents = await _agentLookup.GetAllAgentAsync();
            return agents.Any(x => x.Id == agentId);
        }

        public async Task<bool> SubAgentExistsAsync(int subAgentId, CancellationToken ct = default)
        {
            var subAgents = await _subAgentLookup.GetAllSubAgentAsync();
            return subAgents.Any(x => x.Id == subAgentId);
        }

        public async Task<bool> SalesSegmentExistsAsync(int salesSegmentId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.SalesSegment
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesSegmentId });
            return count > 0;
        }

        public async Task<List<AgentCustomerMappingDto>> GetByCustomerIdAsync(int customerId, CancellationToken ct = default)
        {
            var dp = new DynamicParameters();
            dp.Add("CustomerId", customerId);
            var (accessFilter, _) = await BuildAccessFilterAsync(dp);

            var sql = $@"
                SELECT
                    acm.Id, acm.CustomerId, acm.AgentId, acm.SubAgentId,
                    acm.SalesSegmentId, ss.SegmentName,
                    acm.EffectiveFrom, acm.EffectiveTo, acm.IsDefaultAgent, acm.Remarks,
                    acm.IsActive, acm.IsDeleted,
                    acm.CreatedBy, acm.CreatedDate, acm.CreatedByName, acm.CreatedIP,
                    acm.ModifiedBy, acm.ModifiedDate, acm.ModifiedByName, acm.ModifiedIP
                FROM Sales.AgentCustomerMapping acm
                LEFT JOIN Sales.SalesSegment ss ON acm.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                WHERE acm.CustomerId = @CustomerId AND acm.IsDeleted = 0
                {accessFilter}
                ORDER BY acm.Id DESC";

            var list = (await _dbConnection.QueryAsync<AgentCustomerMappingDto>(
                new CommandDefinition(sql, dp, cancellationToken: ct)))
                .ToList();

            if (list.Any())
            {
                var allCustomers = await _customerLookup.GetAllCustomerAsync();
                var customerDict = allCustomers.ToDictionary(x => x.Id, x => x.CustomerName);

                var allAgents = await _agentLookup.GetAllAgentAsync();
                var agentDict = allAgents.ToDictionary(x => x.Id, x => x.AgentName);

                foreach (var item in list)
                {
                    if (customerDict.TryGetValue(item.CustomerId, out var customerName))
                        item.CustomerName = customerName;

                    if (agentDict.TryGetValue(item.AgentId, out var agentName))
                        item.AgentName = agentName;

                    if (item.SubAgentId.HasValue)
                    {
                        if (agentDict.TryGetValue(item.SubAgentId.Value, out var subAgentName))
                            item.SubAgentName = subAgentName;
                    }
                }
            }

            return list;
        }

        public async Task<bool> MappingAlreadyExistsAsync(int customerId, int agentId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.AgentCustomerMapping
                    WHERE CustomerId = @CustomerId
                      AND AgentId    = @AgentId
                      AND IsDeleted  = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(
                new CommandDefinition(sql, new { CustomerId = customerId, AgentId = agentId }, cancellationToken: ct));
        }

        public async Task<bool> SoftDeleteValidationAsync(int id, CancellationToken ct = default)
        {
            // Returns true if the mapping is referenced in active transactions (blocks delete)
            // Extend this query as Sales Enquiry / Sales Order / Sales Invoice tables reference AgentCustomerMappingId
            const string sql = @"
                SELECT COUNT(1) FROM Sales.AgentCustomerMapping
                WHERE Id = @Id AND IsDeleted = 0";

            // Placeholder: No transaction tables reference AgentCustomerMappingId yet.
            // When Sales Enquiry / Order / Invoice reference this entity, add:
            //   OR EXISTS (SELECT 1 FROM Sales.SalesEnquiryHeader WHERE AgentCustomerMappingId = @Id)
            //   OR EXISTS (SELECT 1 FROM Sales.SalesOrderHeader WHERE AgentCustomerMappingId = @Id)
            // Return false (allow delete) until references are established
            await Task.CompletedTask;
            return false;
        }
    }
}
