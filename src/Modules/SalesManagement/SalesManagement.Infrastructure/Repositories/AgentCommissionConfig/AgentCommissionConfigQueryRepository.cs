using System.Data;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Sales;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Dto;

namespace SalesManagement.Infrastructure.Repositories.AgentCommissionConfig
{
    public class AgentCommissionConfigQueryRepository : IAgentCommissionConfigQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IPaymentTermLookup _paymentTermLookup;
        private readonly ICommissionSplitLookup _commissionSplitLookup;

        public AgentCommissionConfigQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IPaymentTermLookup paymentTermLookup,
            ICommissionSplitLookup commissionSplitLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _paymentTermLookup = paymentTermLookup;
            _commissionSplitLookup = commissionSplitLookup;
        }

        public async Task<(List<AgentCommissionConfigDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (mm.Description LIKE @Search OR cb.Description LIKE @Search OR te.Description LIKE @Search OR cs.SplitCode LIKE @Search OR cs.SplitName LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.AgentCommissionConfig acc
                LEFT JOIN Sales.MiscMaster mm ON acc.CommissionTypeId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cb ON acc.CommissionBasisId = cb.Id AND cb.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster al ON acc.ApplicableLevelId = al.Id AND al.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster te ON acc.TriggerEventId = te.Id AND te.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster st ON acc.SlabTypeId = st.Id AND st.IsDeleted = 0
                LEFT JOIN Sales.CommissionSplit cs ON acc.CommissionSplitId = cs.Id AND cs.IsDeleted = 0
                WHERE acc.IsDeleted = 0
                {searchFilter};

                SELECT
                    acc.Id, acc.AgentId,
                    acc.CommissionTypeId, acc.CommissionBasisId, acc.ApplicableLevelId,
                    acc.CommissionPercentage,
                    acc.ValidityFrom, acc.ValidityTo,
                    acc.TriggerEventId, acc.SlabTypeId, acc.CommissionSplitId,
                    acc.IsActive, acc.IsDeleted,
                    acc.CreatedBy, acc.CreatedDate, acc.CreatedByName, acc.CreatedIP,
                    acc.ModifiedBy, acc.ModifiedDate, acc.ModifiedByName, acc.ModifiedIP,
                    mm.Description AS CommissionTypeName,
                    cb.Description AS CommissionBasisName,
                    al.Description AS ApplicableLevelName,
                    te.Description AS TriggerEventName,
                    st.Description AS SlabTypeName,
                    cs.SplitCode, cs.SplitName
                FROM Sales.AgentCommissionConfig acc
                LEFT JOIN Sales.MiscMaster mm ON acc.CommissionTypeId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cb ON acc.CommissionBasisId = cb.Id AND cb.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster al ON acc.ApplicableLevelId = al.Id AND al.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster te ON acc.TriggerEventId = te.Id AND te.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster st ON acc.SlabTypeId = st.Id AND st.IsDeleted = 0
                LEFT JOIN Sales.CommissionSplit cs ON acc.CommissionSplitId = cs.Id AND cs.IsDeleted = 0
                WHERE acc.IsDeleted = 0
                {searchFilter}
                ORDER BY acc.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<AgentCommissionConfigDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Any())
            {
                // Cross-module lookup — Agent names
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

        public async Task<AgentCommissionConfigDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT
                    acc.Id, acc.AgentId,
                    acc.CommissionTypeId, acc.CommissionBasisId, acc.ApplicableLevelId,
                    acc.CommissionPercentage,
                    acc.ValidityFrom, acc.ValidityTo,
                    acc.TriggerEventId, acc.SlabTypeId, acc.CommissionSplitId,
                    acc.IsActive, acc.IsDeleted,
                    acc.CreatedBy, acc.CreatedDate, acc.CreatedByName, acc.CreatedIP,
                    acc.ModifiedBy, acc.ModifiedDate, acc.ModifiedByName, acc.ModifiedIP,
                    mm.Description AS CommissionTypeName,
                    cb.Description AS CommissionBasisName,
                    al.Description AS ApplicableLevelName,
                    te.Description AS TriggerEventName,
                    st.Description AS SlabTypeName,
                    cs.SplitCode, cs.SplitName
                FROM Sales.AgentCommissionConfig acc
                LEFT JOIN Sales.MiscMaster mm ON acc.CommissionTypeId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cb ON acc.CommissionBasisId = cb.Id AND cb.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster al ON acc.ApplicableLevelId = al.Id AND al.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster te ON acc.TriggerEventId = te.Id AND te.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster st ON acc.SlabTypeId = st.Id AND st.IsDeleted = 0
                LEFT JOIN Sales.CommissionSplit cs ON acc.CommissionSplitId = cs.Id AND cs.IsDeleted = 0
                WHERE acc.Id = @Id AND acc.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<AgentCommissionConfigDto>(
                headerSql, new { Id = id });

            if (dto == null)
                return null;

            // Agent name (cross-module)
            var agents = await _partyLookup.GetByIdsAsync(new[] { dto.AgentId });
            var agentData = agents.FirstOrDefault();
            if (agentData != null)
                dto.AgentName = agentData.PartyName;

            // Fetch SalesGroups children
            const string sgSql = @"
                SELECT asg.Id, asg.SalesGroupId, sg.SalesGroupName
                FROM Sales.AgentCommissionSalesGroup asg
                LEFT JOIN Sales.SalesGroup sg ON asg.SalesGroupId = sg.Id AND sg.IsDeleted = 0
                WHERE asg.AgentCommissionConfigId = @ConfigId AND asg.IsDeleted = 0";

            dto.SalesGroups = (await _dbConnection.QueryAsync<AgentCommissionSalesGroupDto>(
                sgSql, new { ConfigId = id })).ToList();

            // Fetch PaymentTerms children (cross-module names via lookup)
            const string ptSql = @"
                SELECT apt.Id, apt.PaymentTermId
                FROM Sales.AgentCommissionPaymentTerm apt
                WHERE apt.AgentCommissionConfigId = @ConfigId AND apt.IsDeleted = 0";

            var ptRows = (await _dbConnection.QueryAsync<AgentCommissionPaymentTermDto>(
                ptSql, new { ConfigId = id })).ToList();

            if (ptRows.Any())
            {
                var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                var ptDict = paymentTerms.ToDictionary(x => x.Id);

                foreach (var pt in ptRows)
                {
                    if (ptDict.TryGetValue(pt.PaymentTermId, out var ptData))
                    {
                        pt.PaymentTermCode = ptData.Code;
                        pt.PaymentTermDescription = ptData.Description;
                    }
                }
            }
            dto.PaymentTerms = ptRows;

            // Fetch Slabs children
            const string slabSql = @"
                SELECT
                    s.Id, s.SlabOrder, s.FromDelay, s.ToDelay,
                    s.CommissionTypeId, s.CommissionBasisId, s.CommissionValue,
                    ct.Description AS CommissionTypeName,
                    cb2.Description AS CommissionBasisName
                FROM Sales.AgentCommissionSlab s
                LEFT JOIN Sales.MiscMaster ct ON s.CommissionTypeId = ct.Id AND ct.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cb2 ON s.CommissionBasisId = cb2.Id AND cb2.IsDeleted = 0
                WHERE s.AgentCommissionConfigId = @ConfigId AND s.IsDeleted = 0
                ORDER BY s.SlabOrder";

            dto.Slabs = (await _dbConnection.QueryAsync<AgentCommissionSlabDto>(
                slabSql, new { ConfigId = id })).ToList();

            return dto;
        }

        public async Task<IReadOnlyList<AgentCommissionConfigLookupDto>> AutocompleteAsync(
            string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20
                    acc.Id, acc.AgentId,
                    cs.SplitCode
                FROM Sales.AgentCommissionConfig acc
                LEFT JOIN Sales.CommissionSplit cs ON acc.CommissionSplitId = cs.Id AND cs.IsDeleted = 0
                WHERE acc.IsDeleted = 0 AND acc.IsActive = 1
                  AND (cs.SplitCode LIKE @Term OR cs.SplitName LIKE @Term)
                ORDER BY acc.Id DESC";

            var rows = (await _dbConnection.QueryAsync<dynamic>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct)))
                .ToList();

            if (!rows.Any())
                return new List<AgentCommissionConfigLookupDto>();

            var agentIds = rows.Select(r => (int)r.AgentId).Distinct();
            var agents = await _partyLookup.GetByIdsAsync(agentIds, ct);
            var agentDict = agents.ToDictionary(x => x.Id);

            return rows.Select(r =>
            {
                agentDict.TryGetValue((int)r.AgentId, out var agentData);
                return new AgentCommissionConfigLookupDto
                {
                    Id = (int)r.Id,
                    AgentName = agentData?.PartyName,
                    SplitCode = (string?)r.SplitCode
                };
            }).ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.AgentCommissionConfig
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> AgentExistsAsync(int agentId, CancellationToken ct = default)
        {
            var agents = await _partyLookup.GetByIdsAsync(new[] { agentId }, ct);
            return agents.Any();
        }

        public async Task<bool> MiscMasterExistsAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.MiscMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> CommissionSplitExistsAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.CommissionSplit
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> SalesGroupExistsAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.SalesGroup
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> PaymentTermExistsAsync(int id, CancellationToken ct = default)
        {
            var terms = await _paymentTermLookup.GetAllPaymentTermAsync();
            return terms.Any(t => t.Id == id);
        }

        public async Task<bool> OverlapExistsAsync(
            int agentId, int commissionSplitId,
            DateTimeOffset validityFrom, DateTimeOffset? validityTo, int? excludeId = null)
        {
            var sql = @"
                SELECT COUNT(1) FROM Sales.AgentCommissionConfig
                WHERE AgentId = @AgentId
                  AND CommissionSplitId = @CommissionSplitId
                  AND IsDeleted = 0
                  AND IsActive = 1
                  AND ValidityFrom < ISNULL(@ValidityTo, '9999-12-31')
                  AND ISNULL(ValidityTo, '9999-12-31') > @ValidityFrom";

            if (excludeId.HasValue && excludeId.Value > 0)
                sql += " AND Id != @ExcludeId";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                AgentId = agentId,
                CommissionSplitId = commissionSplitId,
                ValidityFrom = validityFrom,
                ValidityTo = validityTo,
                ExcludeId = excludeId
            });
            return count > 0;
        }

        public async Task<bool> IsAgentCommissionConfigLinkedAsync(int id)
        {
            // Currently no dependent tables reference AgentCommissionConfig — return false
            // Update this method when dependent entities are added
            await Task.CompletedTask;
            return false;
        }
    }
}
