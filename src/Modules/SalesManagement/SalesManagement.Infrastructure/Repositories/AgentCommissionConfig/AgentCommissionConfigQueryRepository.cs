using System.Data;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Dto;

namespace SalesManagement.Infrastructure.Repositories.AgentCommissionConfig
{
    public class AgentCommissionConfigQueryRepository : IAgentCommissionConfigQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly ICurrencyLookup _currencyLookup;

        public AgentCommissionConfigQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            ICurrencyLookup currencyLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _currencyLookup = currencyLookup;
        }

        public async Task<(List<AgentCommissionConfigDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (ss.SegmentName LIKE @Search OR mm.Description LIKE @Search OR cb.Description LIKE @Search OR al.Description LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.AgentCommissionConfig acc
                LEFT JOIN Sales.SalesSegment ss ON acc.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON acc.CommissionTypeId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cb ON acc.CommissionBasisId = cb.Id AND cb.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster al ON acc.ApplicableLevelId = al.Id AND al.IsDeleted = 0
                WHERE acc.IsDeleted = 0
                {searchFilter};

                SELECT
                    acc.Id, acc.AgentId, acc.SalesSegmentId,
                    acc.CommissionTypeId, acc.CommissionBasisId, acc.ApplicableLevelId,
                    acc.CommissionPercentage,
                    acc.CurrencyId,
                    acc.ValidityFrom, acc.ValidityTo,
                    acc.IsActive, acc.IsDeleted,
                    acc.CreatedBy, acc.CreatedDate, acc.CreatedByName, acc.CreatedIP,
                    acc.ModifiedBy, acc.ModifiedDate, acc.ModifiedByName, acc.ModifiedIP,
                    ss.SegmentName,
                    mm.Description AS CommissionTypeName,
                    cb.Description AS CommissionBasisName,
                    al.Description AS ApplicableLevelName
                FROM Sales.AgentCommissionConfig acc
                LEFT JOIN Sales.SalesSegment ss ON acc.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON acc.CommissionTypeId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cb ON acc.CommissionBasisId = cb.Id AND cb.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster al ON acc.ApplicableLevelId = al.Id AND al.IsDeleted = 0
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
                // Cross-module lookups — batch by distinct IDs
                var agentIds = list.Select(x => x.AgentId).Distinct();
                var currencyIds = list.Where(x => x.CurrencyId.HasValue).Select(x => x.CurrencyId!.Value).Distinct();

                var agents = await _partyLookup.GetByIdsAsync(agentIds);
                var agentDict = agents.ToDictionary(x => x.Id);

                var currencyDict = new Dictionary<int, string?>();
                if (currencyIds.Any())
                {
                    var currencies = await _currencyLookup.GetByIdsAsync(currencyIds);
                    currencyDict = currencies.ToDictionary(x => x.CurrencyId, x => x.Code);
                }

                foreach (var item in list)
                {
                    if (agentDict.TryGetValue(item.AgentId, out var agentData))
                        item.AgentName = agentData.PartyName;

                    if (item.CurrencyId.HasValue && currencyDict.TryGetValue(item.CurrencyId.Value, out var currencyCode))
                        item.CurrencyCode = currencyCode;
                }
            }

            return (list, totalCount);
        }

        public async Task<AgentCommissionConfigDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    acc.Id, acc.AgentId, acc.SalesSegmentId,
                    acc.CommissionTypeId, acc.CommissionBasisId, acc.ApplicableLevelId,
                    acc.CommissionPercentage,
                    acc.CurrencyId,
                    acc.ValidityFrom, acc.ValidityTo,
                    acc.IsActive, acc.IsDeleted,
                    acc.CreatedBy, acc.CreatedDate, acc.CreatedByName, acc.CreatedIP,
                    acc.ModifiedBy, acc.ModifiedDate, acc.ModifiedByName, acc.ModifiedIP,
                    ss.SegmentName,
                    mm.Description AS CommissionTypeName,
                    cb.Description AS CommissionBasisName,
                    al.Description AS ApplicableLevelName
                FROM Sales.AgentCommissionConfig acc
                LEFT JOIN Sales.SalesSegment ss ON acc.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON acc.CommissionTypeId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cb ON acc.CommissionBasisId = cb.Id AND cb.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster al ON acc.ApplicableLevelId = al.Id AND al.IsDeleted = 0
                WHERE acc.Id = @Id AND acc.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<AgentCommissionConfigDto>(
                sql, new { Id = id });

            if (dto != null)
            {
                var agents = await _partyLookup.GetByIdsAsync(new[] { dto.AgentId });
                var agentData = agents.FirstOrDefault();
                if (agentData != null)
                    dto.AgentName = agentData.PartyName;

                if (dto.CurrencyId.HasValue)
                {
                    var currencies = await _currencyLookup.GetByIdsAsync(new[] { dto.CurrencyId.Value });
                    var currencyData = currencies.FirstOrDefault();
                    if (currencyData != null)
                        dto.CurrencyCode = currencyData.Code;
                }
            }

            return dto;
        }

        public async Task<IReadOnlyList<AgentCommissionConfigLookupDto>> AutocompleteAsync(
            string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20
                    acc.Id, acc.AgentId,
                    ss.SegmentName
                FROM Sales.AgentCommissionConfig acc
                LEFT JOIN Sales.SalesSegment ss ON acc.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                WHERE acc.IsDeleted = 0 AND acc.IsActive = 1
                  AND (ss.SegmentName LIKE @Term)
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
                    SegmentName = (string?)r.SegmentName
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

        public async Task<bool> SalesSegmentExistsAsync(int salesSegmentId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.SalesSegment
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesSegmentId });
            return count > 0;
        }

        public async Task<bool> CommissionTypeExistsAsync(int commissionTypeId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.MiscMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = commissionTypeId });
            return count > 0;
        }

        public async Task<bool> CommissionBasisExistsAsync(int commissionBasisId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.MiscMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = commissionBasisId });
            return count > 0;
        }

        public async Task<bool> ApplicableLevelExistsAsync(int applicableLevelId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.MiscMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = applicableLevelId });
            return count > 0;
        }

        public async Task<bool> CurrencyExistsAsync(int currencyId, CancellationToken ct = default)
        {
            var currencies = await _currencyLookup.GetByIdsAsync(new[] { currencyId }, ct);
            return currencies.Any();
        }

        public async Task<bool> OverlapExistsAsync(
            int agentId, int salesSegmentId,
            DateTimeOffset validityFrom, DateTimeOffset validityTo, int? excludeId = null)
        {
            var sql = @"
                SELECT COUNT(1) FROM Sales.AgentCommissionConfig
                WHERE AgentId = @AgentId
                  AND SalesSegmentId = @SalesSegmentId
                  AND IsDeleted = 0
                  AND IsActive = 1
                  AND ValidityFrom < @ValidityTo
                  AND ValidityTo > @ValidityFrom";

            if (excludeId.HasValue && excludeId.Value > 0)
                sql += " AND Id != @ExcludeId";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                AgentId = agentId,
                SalesSegmentId = salesSegmentId,
                ValidityFrom = validityFrom,
                ValidityTo = validityTo,
                ExcludeId = excludeId
            });
            return count > 0;
        }
    }
}
