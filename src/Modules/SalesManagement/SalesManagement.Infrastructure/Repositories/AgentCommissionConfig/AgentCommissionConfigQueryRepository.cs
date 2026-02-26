using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
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
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly ICurrencyLookup _currencyLookup;

        public AgentCommissionConfigQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IItemLookup itemLookup,
            IUOMLookup uomLookup,
            ICurrencyLookup currencyLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
            _currencyLookup = currencyLookup;
        }

        public async Task<(List<AgentCommissionConfigDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (ss.SegmentName LIKE @Search OR mm.Description LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.AgentCommissionConfig acc
                LEFT JOIN Sales.SalesSegment ss ON acc.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON acc.CommissionTypeId = mm.Id AND mm.IsDeleted = 0
                WHERE acc.IsDeleted = 0
                {searchFilter};

                SELECT
                    acc.Id, acc.AgentId, acc.SalesSegmentId, acc.ItemId,
                    acc.CommissionTypeId, acc.UomId, acc.CommissionPercentage,
                    acc.CurrencyId, acc.SubAgentPercentage,
                    acc.ValidityFrom, acc.ValidityTo,
                    acc.IsActive, acc.IsDeleted,
                    acc.CreatedBy, acc.CreatedDate, acc.CreatedByName, acc.CreatedIP,
                    acc.ModifiedBy, acc.ModifiedDate, acc.ModifiedByName, acc.ModifiedIP,
                    ss.SegmentName,
                    mm.Description AS CommissionTypeName
                FROM Sales.AgentCommissionConfig acc
                LEFT JOIN Sales.SalesSegment ss ON acc.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON acc.CommissionTypeId = mm.Id AND mm.IsDeleted = 0
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
                var itemIds = list.Select(x => x.ItemId).Distinct();
                var uomIds = list.Where(x => x.UomId.HasValue).Select(x => x.UomId!.Value).Distinct();
                var currencyIds = list.Where(x => x.CurrencyId.HasValue).Select(x => x.CurrencyId!.Value).Distinct();

                var agents = await _partyLookup.GetByIdsAsync(agentIds);
                var agentDict = agents.ToDictionary(x => x.Id);

                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(x => x.Id);

                var uomDict = new Dictionary<int, string>();
                if (uomIds.Any())
                {
                    var uoms = await _uomLookup.GetByIdsAsync(uomIds);
                    uomDict = uoms.ToDictionary(x => x.Id, x => x.UOMName);
                }

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

                    if (itemDict.TryGetValue(item.ItemId, out var itemData))
                        item.ItemName = itemData.ItemName;

                    if (item.UomId.HasValue && uomDict.TryGetValue(item.UomId.Value, out var uomName))
                        item.UomName = uomName;

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
                    acc.Id, acc.AgentId, acc.SalesSegmentId, acc.ItemId,
                    acc.CommissionTypeId, acc.UomId, acc.CommissionPercentage,
                    acc.CurrencyId, acc.SubAgentPercentage,
                    acc.ValidityFrom, acc.ValidityTo,
                    acc.IsActive, acc.IsDeleted,
                    acc.CreatedBy, acc.CreatedDate, acc.CreatedByName, acc.CreatedIP,
                    acc.ModifiedBy, acc.ModifiedDate, acc.ModifiedByName, acc.ModifiedIP,
                    ss.SegmentName,
                    mm.Description AS CommissionTypeName
                FROM Sales.AgentCommissionConfig acc
                LEFT JOIN Sales.SalesSegment ss ON acc.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON acc.CommissionTypeId = mm.Id AND mm.IsDeleted = 0
                WHERE acc.Id = @Id AND acc.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<AgentCommissionConfigDto>(
                sql, new { Id = id });

            if (dto != null)
            {
                var agents = await _partyLookup.GetByIdsAsync(new[] { dto.AgentId });
                var agentData = agents.FirstOrDefault();
                if (agentData != null)
                    dto.AgentName = agentData.PartyName;

                var items = await _itemLookup.GetByIdsAsync(new[] { dto.ItemId });
                var itemData = items.FirstOrDefault();
                if (itemData != null)
                    dto.ItemName = itemData.ItemName;

                if (dto.UomId.HasValue)
                {
                    var uoms = await _uomLookup.GetByIdsAsync(new[] { dto.UomId.Value });
                    var uomData = uoms.FirstOrDefault();
                    if (uomData != null)
                        dto.UomName = uomData.UOMName;
                }

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
                    acc.Id, acc.AgentId, acc.ItemId,
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
            var itemIds = rows.Select(r => (int)r.ItemId).Distinct();

            var agents = await _partyLookup.GetByIdsAsync(agentIds, ct);
            var agentDict = agents.ToDictionary(x => x.Id);

            var items = await _itemLookup.GetByIdsAsync(itemIds, ct);
            var itemDict = items.ToDictionary(x => x.Id);

            return rows.Select(r =>
            {
                agentDict.TryGetValue((int)r.AgentId, out var agentData);
                itemDict.TryGetValue((int)r.ItemId, out var itemData);
                return new AgentCommissionConfigLookupDto
                {
                    Id = (int)r.Id,
                    AgentName = agentData?.PartyName,
                    ItemName = itemData?.ItemName,
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

        public async Task<bool> ItemExistsAsync(int itemId, CancellationToken ct = default)
        {
            var items = await _itemLookup.GetByIdsAsync(new[] { itemId }, ct);
            return items.Any();
        }

        public async Task<bool> CommissionTypeExistsAsync(int commissionTypeId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Sales.MiscMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = commissionTypeId });
            return count > 0;
        }

        public async Task<bool> UomExistsAsync(int uomId, CancellationToken ct = default)
        {
            var uoms = await _uomLookup.GetByIdsAsync(new[] { uomId }, ct);
            return uoms.Any();
        }

        public async Task<bool> CurrencyExistsAsync(int currencyId, CancellationToken ct = default)
        {
            var currencies = await _currencyLookup.GetByIdsAsync(new[] { currencyId }, ct);
            return currencies.Any();
        }

        public async Task<bool> OverlapExistsAsync(
            int agentId, int salesSegmentId, int itemId,
            DateTimeOffset validityFrom, DateTimeOffset validityTo, int? excludeId = null)
        {
            var sql = @"
                SELECT COUNT(1) FROM Sales.AgentCommissionConfig
                WHERE AgentId = @AgentId
                  AND SalesSegmentId = @SalesSegmentId
                  AND ItemId = @ItemId
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
                ItemId = itemId,
                ValidityFrom = validityFrom,
                ValidityTo = validityTo,
                ExcludeId = excludeId
            });
            return count > 0;
        }
    }
}
