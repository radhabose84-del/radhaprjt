using System.Data;
using Contracts.Interfaces;
using Dapper;
using SalesManagement.Application.Common.Interfaces;

namespace SalesManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of <see cref="IMarketingOfficerAccessFilter"/>.
    /// Reads <c>EmpId</c> from <see cref="IIPAddressService"/> and resolves the officer's
    /// accessible Agent and Customer IDs via <c>Sales.OfficerAgent</c> and <c>Sales.AgentCustomerMapping</c>.
    /// Caches results per request (Scoped lifetime) so repositories pay the lookup cost at most once per HTTP request.
    /// </summary>
    public class MarketingOfficerAccessFilter : IMarketingOfficerAccessFilter
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        private IReadOnlyList<int>? _cachedAgentIds;
        private IReadOnlyList<int>? _cachedCustomerIds;

        public MarketingOfficerAccessFilter(
            IDbConnection dbConnection,
            IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public bool IsMarketingOfficer() => _ipAddressService.GetEmpId().HasValue;

        public int? GetCurrentMarketingOfficerId() => _ipAddressService.GetEmpId();

        public async Task<IReadOnlyList<int>> GetAccessibleAgentIdsAsync(CancellationToken ct = default)
        {
            if (_cachedAgentIds != null)
                return _cachedAgentIds;

            var empId = _ipAddressService.GetEmpId();
            if (!empId.HasValue)
            {
                _cachedAgentIds = Array.Empty<int>();
                return _cachedAgentIds;
            }

            const string sql = @"
                SELECT DISTINCT AgentId
                FROM Sales.OfficerAgent
                WHERE MarketingOfficerId = @EmpId
                  AND IsActive = 1
                  AND CAST(GETDATE() AS date) BETWEEN ValidityFrom AND ValidityTo;";

            var rows = await _dbConnection.QueryAsync<int>(sql, new { EmpId = empId.Value });
            _cachedAgentIds = rows.ToList();
            return _cachedAgentIds;
        }

        public async Task<IReadOnlyList<int>> GetAccessibleCustomerIdsAsync(CancellationToken ct = default)
        {
            if (_cachedCustomerIds != null)
                return _cachedCustomerIds;

            var empId = _ipAddressService.GetEmpId();
            if (!empId.HasValue)
            {
                _cachedCustomerIds = Array.Empty<int>();
                return _cachedCustomerIds;
            }

            const string sql = @"
                SELECT DISTINCT acm.CustomerId
                FROM Sales.AgentCustomerMapping acm
                INNER JOIN Sales.OfficerAgent oa ON acm.AgentId = oa.AgentId
                WHERE oa.MarketingOfficerId = @EmpId
                  AND oa.IsActive = 1
                  AND CAST(GETDATE() AS date) BETWEEN oa.ValidityFrom AND oa.ValidityTo
                  AND acm.IsActive = 1
                  AND acm.IsDeleted = 0;";

            var rows = await _dbConnection.QueryAsync<int>(sql, new { EmpId = empId.Value });
            _cachedCustomerIds = rows.ToList();
            return _cachedCustomerIds;
        }

        public async Task<bool> CanAccessCustomerAsync(int customerId, CancellationToken ct = default)
        {
            if (!IsMarketingOfficer())
                return true;

            var customers = await GetAccessibleCustomerIdsAsync(ct);
            return customers.Contains(customerId);
        }

        public async Task<bool> CanAccessAgentAsync(int agentId, CancellationToken ct = default)
        {
            if (!IsMarketingOfficer())
                return true;

            var agents = await GetAccessibleAgentIdsAsync(ct);
            return agents.Contains(agentId);
        }
    }
}
