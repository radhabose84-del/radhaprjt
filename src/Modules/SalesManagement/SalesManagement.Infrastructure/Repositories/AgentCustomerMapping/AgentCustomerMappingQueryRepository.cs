using System.Data;
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

        public AgentCustomerMappingQueryRepository(
            IDbConnection dbConnection,
            ICustomerLookup customerLookup,
            IAgentLookup agentLookup)
        {
            _dbConnection = dbConnection;
            _customerLookup = customerLookup;
            _agentLookup = agentLookup;
        }

        public async Task<(List<AgentCustomerMappingDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (acm.Remarks LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.AgentCustomerMapping acm
                WHERE acm.IsDeleted = 0
                {searchFilter};

                SELECT
                    acm.Id, acm.CustomerId, acm.AgentId, acm.SubAgentId,
                    acm.EffectiveFrom, acm.EffectiveTo, acm.IsDefaultAgent, acm.Remarks,
                    acm.IsActive, acm.IsDeleted,
                    acm.CreatedBy, acm.CreatedDate, acm.CreatedByName, acm.CreatedIP,
                    acm.ModifiedBy, acm.ModifiedDate, acm.ModifiedByName, acm.ModifiedIP
                FROM Sales.AgentCustomerMapping acm
                WHERE acm.IsDeleted = 0
                {searchFilter}
                ORDER BY acm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
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

        public async Task<AgentCustomerMappingDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    acm.Id, acm.CustomerId, acm.AgentId, acm.SubAgentId,
                    acm.EffectiveFrom, acm.EffectiveTo, acm.IsDefaultAgent, acm.Remarks,
                    acm.IsActive, acm.IsDeleted,
                    acm.CreatedBy, acm.CreatedDate, acm.CreatedByName, acm.CreatedIP,
                    acm.ModifiedBy, acm.ModifiedDate, acm.ModifiedByName, acm.ModifiedIP
                FROM Sales.AgentCustomerMapping acm
                WHERE acm.Id = @Id AND acm.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<AgentCustomerMappingDto>(
                sql, new { Id = id });

            if (dto != null)
            {
                var allCustomers = await _customerLookup.GetAllCustomerAsync();
                var customer = allCustomers.FirstOrDefault(x => x.Id == dto.CustomerId);
                dto.CustomerName = customer?.CustomerName;

                var allAgents = await _agentLookup.GetAllAgentAsync();
                var agentDict = allAgents.ToDictionary(x => x.Id, x => x.AgentName);

                if (agentDict.TryGetValue(dto.AgentId, out var agentName))
                    dto.AgentName = agentName;

                if (dto.SubAgentId.HasValue && agentDict.TryGetValue(dto.SubAgentId.Value, out var subAgentName))
                    dto.SubAgentName = subAgentName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<AgentCustomerMappingLookupDto>> AutocompleteAsync(
            string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20
                    acm.Id, acm.CustomerId, acm.AgentId
                FROM Sales.AgentCustomerMapping acm
                WHERE acm.IsDeleted = 0 AND acm.IsActive = 1
                ORDER BY acm.Id DESC";

            var rows = (await _dbConnection.QueryAsync<dynamic>(
                new CommandDefinition(sql, cancellationToken: ct)))
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
