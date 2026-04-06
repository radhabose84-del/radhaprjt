using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.ComplaintResolution.Dto;

namespace SalesManagement.Infrastructure.Repositories.ComplaintResolution
{
    public class ComplaintResolutionQueryRepository : IComplaintResolutionQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUserLookup _userLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;

        public ComplaintResolutionQueryRepository(
            IDbConnection dbConnection,
            IUserLookup userLookup,
            IPartyLookup partyLookup,
            IItemLookup itemLookup)
        {
            _dbConnection = dbConnection;
            _userLookup = userLookup;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
        }

        public async Task<(List<ResolutionListDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, string? statusFilter)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : @" AND (ch.ComplaintNumber LIKE @SearchTerm OR ch.Remarks LIKE @SearchTerm)";

            // Status filter: Open = no resolution yet, Ready for Closure, Closed
            var statusCondition = string.Empty;
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                if (statusFilter.Equals("Open", StringComparison.OrdinalIgnoreCase))
                    statusCondition = " AND cr.Id IS NULL";
                else
                    statusCondition = " AND cs.Description = @StatusFilter";
            }

            // Show complaints where QC is accepted (feedback completed stage or beyond)
            var baseSql = @"
                FROM Sales.ComplaintHeader ch
                INNER JOIN Sales.ComplaintQCReview qr ON qr.ComplaintHeaderId = ch.Id AND qr.IsDeleted = 0
                INNER JOIN Sales.MiscMaster qcs ON qr.ComplaintStatusId = qcs.Id AND qcs.IsDeleted = 0 AND qcs.Code = 'QC Accepted'
                LEFT JOIN Sales.ComplaintResolution cr ON cr.ComplaintHeaderId = ch.Id AND cr.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster rt ON cr.ResolutionTypeId = rt.Id AND rt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cs ON cr.ClosureStatusId = cs.Id AND cs.IsDeleted = 0
                WHERE ch.IsDeleted = 0";

            var countSql = $"SELECT COUNT(*) {baseSql} {searchFilter} {statusCondition};";

            var dataSql = $@"
                SELECT
                    cr.Id AS ResolutionId,
                    ch.Id AS ComplaintHeaderId,
                    ch.ComplaintNumber,
                    ch.ComplaintDate,
                    ch.CustomerId,
                    rt.Description AS ResolutionTypeName,
                    CASE
                        WHEN cr.Id IS NULL THEN 'Open'
                        ELSE ISNULL(cs.Description, 'Open')
                    END AS ClosureStatusName,
                    cr.ResolvedBy,
                    cr.ResolvedDate
                {baseSql} {searchFilter} {statusCondition}
                ORDER BY ch.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                SearchTerm = $"%{searchTerm}%",
                StatusFilter = statusFilter,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<ResolutionListDto>(dataSql, parameters)).ToList();

            if (data.Count > 0)
            {
                // Populate CustomerName
                var customerIds = data.Select(d => d.CustomerId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(customerIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                // Populate ResolvedByName
                var resolvedByIds = data.Where(d => d.ResolvedBy.HasValue).Select(d => d.ResolvedBy!.Value).Distinct().ToList();
                Dictionary<int, string> userDict = new();
                if (resolvedByIds.Count > 0)
                {
                    var users = await _userLookup.GetByIdsAsync(resolvedByIds);
                    userDict = users.ToDictionary(u => u.UserId, u => $"{u.FirstName} {u.LastName}".Trim());
                }

                // Populate ProductName from first complaint detail
                var complaintIds = data.Select(d => d.ComplaintHeaderId).Distinct().ToList();
                const string itemSql = @"
                    SELECT cd.ComplaintHeaderId, cd.ItemId
                    FROM Sales.ComplaintDetail cd
                    WHERE cd.ComplaintHeaderId IN @Ids AND cd.IsDeleted = 0;";
                var detailItems = (await _dbConnection.QueryAsync<(int ComplaintHeaderId, int ItemId)>(
                    itemSql, new { Ids = complaintIds })).ToList();

                Dictionary<int, string> productDict = new();
                if (detailItems.Count > 0)
                {
                    var itemIds = detailItems.Select(d => d.ItemId).Distinct();
                    var items = await _itemLookup.GetByIdsAsync(itemIds);
                    var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                    var complaintItemDict = detailItems
                        .GroupBy(d => d.ComplaintHeaderId)
                        .ToDictionary(g => g.Key, g => g.First().ItemId);

                    foreach (var kvp in complaintItemDict)
                    {
                        if (itemDict.TryGetValue(kvp.Value, out var name))
                            productDict[kvp.Key] = name;
                    }
                }

                foreach (var item in data)
                {
                    item.CustomerName = partyDict.TryGetValue(item.CustomerId, out var custName) ? custName : null;
                    item.ProductName = productDict.TryGetValue(item.ComplaintHeaderId, out var prodName) ? prodName : null;

                    if (item.ResolvedBy.HasValue && userDict.TryGetValue(item.ResolvedBy.Value, out var resolverName))
                        item.ResolvedByName = resolverName;
                }
            }

            return (data, totalCount);
        }

        public async Task<ComplaintResolutionDto?> GetByIdAsync(int id)
        {
            return await GetResolutionAsync("cr.Id = @Id", new { Id = id });
        }

        public async Task<ComplaintResolutionDto?> GetByComplaintHeaderIdAsync(int complaintHeaderId)
        {
            return await GetResolutionAsync("cr.ComplaintHeaderId = @ComplaintHeaderId", new { ComplaintHeaderId = complaintHeaderId });
        }

        private async Task<ComplaintResolutionDto?> GetResolutionAsync(string whereClause, object parameters)
        {
            var sql = $@"
                SELECT
                    cr.Id,
                    cr.ComplaintHeaderId,
                    ch.ComplaintNumber,
                    ch.ComplaintDate,
                    ch.CustomerId,
                    cr.ResolutionTypeId,
                    rt.Description AS ResolutionTypeName,
                    cr.ResolutionSummary,
                    cr.ReturnQuantity,
                    cr.ReturnLocationId,
                    rl.Description AS ReturnLocationName,
                    cr.ReturnStatusId,
                    rs.Description AS ReturnStatusName,
                    cr.CreditAmount,
                    cr.FinanceReference,
                    cr.ReplacementQuantity,
                    cr.DispatchReference,
                    cr.ActionDescription,
                    cr.ClosureStatusId,
                    cs.Description AS ClosureStatusName,
                    cr.ClosureRemarks,
                    cr.ResolvedBy,
                    cr.ResolvedDate,
                    cr.ClosedBy,
                    cr.ClosedDate,
                    cr.IsActive,
                    cr.IsDeleted
                FROM Sales.ComplaintResolution cr
                INNER JOIN Sales.ComplaintHeader ch ON cr.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster rt ON cr.ResolutionTypeId = rt.Id AND rt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster rl ON cr.ReturnLocationId = rl.Id AND rl.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster rs ON cr.ReturnStatusId = rs.Id AND rs.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cs ON cr.ClosureStatusId = cs.Id AND cs.IsDeleted = 0
                WHERE cr.IsDeleted = 0 AND {whereClause};";

            var rows = (await _dbConnection.QueryAsync(sql, parameters)).ToList();
            if (rows.Count == 0)
                return null;

            var row = rows[0];
            var resolution = new ComplaintResolutionDto
            {
                Id = (int)row.Id,
                ComplaintHeaderId = (int)row.ComplaintHeaderId,
                ComplaintNumber = (string?)row.ComplaintNumber,
                ComplaintDate = row.ComplaintDate != null ? DateOnly.FromDateTime((DateTime)row.ComplaintDate) : null,
                ResolutionTypeId = (int)row.ResolutionTypeId,
                ResolutionTypeName = (string?)row.ResolutionTypeName,
                ResolutionSummary = (string?)row.ResolutionSummary,
                ReturnQuantity = (decimal?)row.ReturnQuantity,
                ReturnLocationId = (int?)row.ReturnLocationId,
                ReturnLocationName = (string?)row.ReturnLocationName,
                ReturnStatusId = (int?)row.ReturnStatusId,
                ReturnStatusName = (string?)row.ReturnStatusName,
                CreditAmount = (decimal?)row.CreditAmount,
                FinanceReference = (string?)row.FinanceReference,
                ReplacementQuantity = (decimal?)row.ReplacementQuantity,
                DispatchReference = (string?)row.DispatchReference,
                ActionDescription = (string?)row.ActionDescription,
                ClosureStatusId = (int?)row.ClosureStatusId,
                ClosureStatusName = (string?)row.ClosureStatusName,
                ClosureRemarks = (string?)row.ClosureRemarks,
                ResolvedBy = (int?)row.ResolvedBy,
                ResolvedDate = (DateTimeOffset?)row.ResolvedDate,
                ClosedBy = (int?)row.ClosedBy,
                ClosedDate = (DateTimeOffset?)row.ClosedDate,
                IsActive = (bool)row.IsActive,
                IsDeleted = (bool)row.IsDeleted
            };

            int? customerId = (int?)row.CustomerId;

            // Populate cross-module user names
            var userIds = new List<int>();
            if (resolution.ResolvedBy.HasValue) userIds.Add(resolution.ResolvedBy.Value);
            if (resolution.ClosedBy.HasValue) userIds.Add(resolution.ClosedBy.Value);

            if (userIds.Count > 0)
            {
                var users = await _userLookup.GetByIdsAsync(userIds.Distinct());
                var userDict = users.ToDictionary(u => u.UserId, u => $"{u.FirstName} {u.LastName}".Trim());

                if (resolution.ResolvedBy.HasValue && userDict.TryGetValue(resolution.ResolvedBy.Value, out var resolvedName))
                    resolution.ResolvedByName = resolvedName;

                if (resolution.ClosedBy.HasValue && userDict.TryGetValue(resolution.ClosedBy.Value, out var closedName))
                    resolution.ClosedByName = closedName;
            }

            // Populate customer name
            if (customerId.HasValue)
            {
                var party = await _partyLookup.GetByIdAsync(customerId.Value);
                resolution.CustomerName = party?.PartyName;
            }

            // Populate item name and complaint quantity from first complaint detail
            var detailSql = @"
                SELECT TOP 1 cd.ItemId, cd.NetWeight
                FROM Sales.ComplaintDetail cd
                WHERE cd.ComplaintHeaderId = @ComplaintHeaderId AND cd.IsDeleted = 0;";
            var detail = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(detailSql, new { resolution.ComplaintHeaderId });
            if (detail != null)
            {
                int? itemId = (int?)detail.ItemId;
                if (itemId.HasValue)
                {
                    var items = await _itemLookup.GetByIdsAsync(new[] { itemId.Value });
                    var item = items.FirstOrDefault();
                    resolution.ItemName = item?.ItemName;
                }
                resolution.ComplaintQuantity = (decimal?)detail.NetWeight;
            }

            return resolution;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.ComplaintResolution WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> ResolutionExistsForComplaintAsync(int complaintHeaderId, int? excludeId = null)
        {
            var sql = excludeId.HasValue
                ? "SELECT COUNT(1) FROM Sales.ComplaintResolution WHERE ComplaintHeaderId = @ComplaintHeaderId AND Id != @ExcludeId AND IsDeleted = 0;"
                : "SELECT COUNT(1) FROM Sales.ComplaintResolution WHERE ComplaintHeaderId = @ComplaintHeaderId AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql,
                new { ComplaintHeaderId = complaintHeaderId, ExcludeId = excludeId ?? 0 });
            return count > 0;
        }

        public async Task<bool> ComplaintExistsAsync(int complaintHeaderId)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.ComplaintHeader WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = complaintHeaderId });
            return count > 0;
        }

        public async Task<bool> MiscMasterExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.MiscMaster WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }
    }
}
