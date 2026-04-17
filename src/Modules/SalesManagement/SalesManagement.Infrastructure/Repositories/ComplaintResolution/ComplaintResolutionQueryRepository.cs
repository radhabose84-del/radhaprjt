using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
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
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;
        private readonly IDataAccessFilter _dataAccessFilter;

        public ComplaintResolutionQueryRepository(
            IDbConnection dbConnection,
            IUserLookup userLookup,
            IPartyLookup partyLookup,
            IItemLookup itemLookup,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup,
            IDataAccessFilter dataAccessFilter)
        {
            _dbConnection = dbConnection;
            _userLookup = userLookup;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
            _warehouseLookup = warehouseLookup;
            _binLookup = binLookup;
            _dataAccessFilter = dataAccessFilter;
        }

        public async Task<(List<ResolutionListDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, string? statusFilter)
        {
            // Data access control
            var accessCtx = await _dataAccessFilter.GetContextAsync();
            if (accessCtx.IsCustomerRestricted && accessCtx.AllowedCustomerIds.Count == 0)
                return (new List<ResolutionListDto>(), 0);

            var partyFilter = accessCtx.IsCustomerRestricted && accessCtx.AllowedCustomerIds.Count > 0
                ? " AND ch.CustomerId IN @AllowedCustomerIds"
                : string.Empty;

            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : @" AND (ch.ComplaintNumber LIKE @SearchTerm OR cr.ResolutionSummary LIKE @SearchTerm)";

            var statusFilterSql = string.IsNullOrWhiteSpace(statusFilter)
                ? string.Empty
                : " AND cs.Description = @StatusFilter";

            var allFilters = $"{searchFilter}{partyFilter}{statusFilterSql}";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.ComplaintResolution cr
                INNER JOIN Sales.ComplaintHeader ch ON cr.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cs ON cr.ClosureStatusId = cs.Id AND cs.IsDeleted = 0
                WHERE cr.IsDeleted = 0 {allFilters};";

            var dataSql = $@"
                SELECT
                    cr.Id AS ResolutionId,
                    cr.ComplaintHeaderId,
                    ch.ComplaintNumber,
                    ch.ComplaintDate,
                    ch.CustomerId,
                    rt.Description AS ResolutionTypeName,
                    ISNULL(cs.Description, 'Open') AS ClosureStatusName,
                    cr.ResolvedBy,
                    cr.ResolvedDate
                FROM Sales.ComplaintResolution cr
                INNER JOIN Sales.ComplaintHeader ch ON cr.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster rt ON cr.ResolutionTypeId = rt.Id AND rt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cs ON cr.ClosureStatusId = cs.Id AND cs.IsDeleted = 0
                WHERE cr.IsDeleted = 0 {allFilters}
                ORDER BY cr.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var dp = new DynamicParameters();
            dp.Add("SearchTerm", $"%{searchTerm}%");
            dp.Add("StatusFilter", statusFilter);
            dp.Add("Offset", (pageNumber - 1) * pageSize);
            dp.Add("PageSize", pageSize);
            if (accessCtx.IsCustomerRestricted && accessCtx.AllowedCustomerIds.Count > 0)
                dp.Add("AllowedCustomerIds", accessCtx.AllowedCustomerIds.ToList());

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, dp);
            var data = (await _dbConnection.QueryAsync<ResolutionListDto>(dataSql, dp)).ToList();

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

        public async Task<ComplaintResolutionFormDataDto?> GetFormDataByComplaintIdAsync(int complaintHeaderId)
        {
            // 1. Complaint header info
            const string headerSql = @"
                SELECT
                    ch.Id AS ComplaintHeaderId,
                    ch.ComplaintNumber,
                    ch.ComplaintDate,
                    ch.CustomerId,
                    ch.Remarks AS ComplaintRemarks
                FROM Sales.ComplaintHeader ch
                WHERE ch.Id = @Id AND ch.IsDeleted = 0;";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<ComplaintResolutionFormDataDto>(headerSql, new { Id = complaintHeaderId });
            if (dto == null) return null;

            // Populate CustomerName
            if (dto.CustomerId > 0)
            {
                var party = await _partyLookup.GetByIdAsync(dto.CustomerId);
                dto.CustomerName = party?.PartyName;
            }

            // 2. QC Review info
            const string qcSql = @"
                SELECT
                    qcs.Description AS QCStatusName,
                    sev.Description AS SeverityName,
                    comp.Description AS CompensationStructureName,
                    qr.Comments AS QCComments,
                    qr.ExpectedResolutionDate,
                    (SELECT COUNT(*) FROM Sales.ComplaintQCReviewAssignment a WHERE a.ComplaintQCReviewId = qr.Id AND a.IsDeleted = 0) AS TotalAssignments,
                    (SELECT COUNT(*) FROM Sales.ComplaintQCReviewAssignment a
                     INNER JOIN Sales.ComplaintDepartmentFeedback f ON f.AssignmentId = a.Id AND f.IsDeleted = 0
                     WHERE a.ComplaintQCReviewId = qr.Id AND a.IsDeleted = 0) AS SubmittedFeedbacks
                FROM Sales.ComplaintQCReview qr
                LEFT JOIN Sales.MiscMaster qcs ON qr.ComplaintStatusId = qcs.Id AND qcs.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sev ON qr.SeverityId = sev.Id AND sev.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster comp ON qr.CompensationStructureId = comp.Id AND comp.IsDeleted = 0
                WHERE qr.ComplaintHeaderId = @Id AND qr.IsDeleted = 0;";

            var qcData = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(qcSql, new { Id = complaintHeaderId });
            if (qcData != null)
            {
                dto.QCStatusName = (string?)qcData.QCStatusName;
                dto.SeverityName = (string?)qcData.SeverityName;
                dto.CompensationStructureName = (string?)qcData.CompensationStructureName;
                dto.QCComments = (string?)qcData.QCComments;
                dto.ExpectedResolutionDate = qcData.ExpectedResolutionDate != null ? DateOnly.FromDateTime((DateTime)qcData.ExpectedResolutionDate) : null;
                dto.TotalAssignments = (int?)qcData.TotalAssignments ?? 0;
                dto.SubmittedFeedbacks = (int?)qcData.SubmittedFeedbacks ?? 0;
            }

            // 3. Feedback summary
            const string feedbackSql = @"
                SELECT
                    role.Description AS RoleName,
                    a.ResponsiblePersonId,
                    f.RootCauseText,
                    f.CorrectiveAction,
                    f.PreventiveAction,
                    CASE
                        WHEN f.Id IS NULL THEN 'Pending'
                        ELSE fs.Description
                    END AS FeedbackStatusName
                FROM Sales.ComplaintQCReviewAssignment a
                INNER JOIN Sales.ComplaintQCReview qr ON a.ComplaintQCReviewId = qr.Id AND qr.IsDeleted = 0
                LEFT JOIN Sales.ComplaintDepartmentFeedback f ON f.AssignmentId = a.Id AND f.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster role ON a.RoleId = role.Id AND role.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster fs ON f.FeedbackStatusId = fs.Id AND fs.IsDeleted = 0
                WHERE qr.ComplaintHeaderId = @Id AND a.IsDeleted = 0;";

            var feedbacks = (await _dbConnection.QueryAsync<FeedbackSummaryDto>(feedbackSql, new { Id = complaintHeaderId })).ToList();

            // Populate ResponsiblePersonName
            if (feedbacks.Count > 0)
            {
                var personSql = @"
                    SELECT a.ResponsiblePersonId
                    FROM Sales.ComplaintQCReviewAssignment a
                    INNER JOIN Sales.ComplaintQCReview qr ON a.ComplaintQCReviewId = qr.Id AND qr.IsDeleted = 0
                    WHERE qr.ComplaintHeaderId = @Id AND a.IsDeleted = 0;";
                var personIds = (await _dbConnection.QueryAsync<int>(personSql, new { Id = complaintHeaderId })).Distinct().ToList();

                if (personIds.Count > 0)
                {
                    var users = await _userLookup.GetByIdsAsync(personIds);
                    var userDict = users.ToDictionary(u => u.UserId, u => $"{u.FirstName} {u.LastName}".Trim());

                    // Re-query to get ResponsiblePersonId mapping
                    var assignmentSql = @"
                        SELECT a.Id, a.ResponsiblePersonId
                        FROM Sales.ComplaintQCReviewAssignment a
                        INNER JOIN Sales.ComplaintQCReview qr ON a.ComplaintQCReviewId = qr.Id AND qr.IsDeleted = 0
                        WHERE qr.ComplaintHeaderId = @Id AND a.IsDeleted = 0
                        ORDER BY a.Id;";
                    var assignments = (await _dbConnection.QueryAsync<(int Id, int ResponsiblePersonId)>(assignmentSql, new { Id = complaintHeaderId })).ToList();

                    for (int i = 0; i < feedbacks.Count && i < assignments.Count; i++)
                    {
                        if (userDict.TryGetValue(assignments[i].ResponsiblePersonId, out var name))
                            feedbacks[i].ResponsiblePersonName = name;
                    }
                }
            }

            dto.Feedbacks = feedbacks;

            // 4. Complaint items
            const string itemsSql = @"
                SELECT
                    cd.InvoiceHeaderId,
                    ih.InvoiceNo,
                    ih.InvoiceDate,
                    cd.ItemId,
                    cd.LotId,
                    cd.NumberOfPacks,
                    cd.NetWeight,
                    cd.InvoiceAmount
                FROM Sales.ComplaintDetail cd
                INNER JOIN Sales.InvoiceHeader ih ON cd.InvoiceHeaderId = ih.Id AND ih.IsDeleted = 0
                WHERE cd.ComplaintHeaderId = @Id AND cd.IsDeleted = 0;";

            var items = (await _dbConnection.QueryAsync<ComplaintItemDto>(itemsSql, new { Id = complaintHeaderId })).ToList();

            if (items.Count > 0)
            {
                var itemIds = items.Select(i => i.ItemId).Distinct();
                var itemLookups = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = itemLookups.ToDictionary(i => i.Id, i => i.ItemName);

                foreach (var item in items)
                    item.ItemName = itemDict.TryGetValue(item.ItemId, out var name) ? name : null;
            }

            dto.Items = items;

            // 5. Max return quantity = total NumberOfPacks across all complaint items
            const string dispatchQtySql = @"
                SELECT ISNULL(SUM(cd.NumberOfPacks), 0)
                FROM Sales.ComplaintDetail cd
                WHERE cd.ComplaintHeaderId = @Id AND cd.IsDeleted = 0;";

            dto.MaxReturnQuantity = await _dbConnection.ExecuteScalarAsync<decimal>(dispatchQtySql, new { Id = complaintHeaderId });

            // Get warehouse/bin from StockLedger (dispatched packs for this complaint's items)
            const string stockLocationSql = @"
                SELECT TOP 1 sl.WarehouseId, sl.BinId
                FROM Sales.StockLedger sl
                INNER JOIN Sales.ComplaintDetail cd ON sl.ItemId = cd.ItemId
                    AND (sl.LotId = cd.LotId OR (cd.LotId IS NULL))
                WHERE cd.ComplaintHeaderId = @Id AND cd.IsDeleted = 0
                    AND sl.DocType = 'PROD'
                ORDER BY sl.Id DESC;";

            var stockLocation = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(stockLocationSql, new { Id = complaintHeaderId });
            if (stockLocation != null)
            {
                dto.DispatchWarehouseId = (int?)stockLocation.WarehouseId;
                dto.DispatchBinId = (int?)stockLocation.BinId;

                if (dto.DispatchWarehouseId.HasValue)
                {
                    var warehouses = await _warehouseLookup.GetByIdsAsync(new[] { dto.DispatchWarehouseId.Value });
                    dto.DispatchWarehouseName = warehouses.FirstOrDefault()?.WarehouseName;
                }

                if (dto.DispatchBinId.HasValue)
                {
                    var bins = await _binLookup.GetByIdsAsync(new[] { dto.DispatchBinId.Value });
                    dto.DispatchBinName = bins.FirstOrDefault()?.BinName;
                }
            }

            // Get Sales Return reference if exists
            const string srRefSql = @"
                SELECT TOP 1 ReturnNumber
                FROM Sales.SalesReturnHeader
                WHERE ComplaintHeaderId = @Id AND IsDeleted = 0
                ORDER BY Id DESC;";
            dto.SalesReturnReference = await _dbConnection.ExecuteScalarAsync<string?>(srRefSql, new { Id = complaintHeaderId });

            // 6. Existing resolution (if any)
            dto.ExistingResolution = await GetByComplaintHeaderIdAsync(complaintHeaderId);

            return dto;
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

            // Populate item name and total complaint quantity from complaint details
            var detailSql = @"
                SELECT TOP 1 cd.ItemId
                FROM Sales.ComplaintDetail cd
                WHERE cd.ComplaintHeaderId = @ComplaintHeaderId AND cd.IsDeleted = 0;";
            var itemIdResult = await _dbConnection.ExecuteScalarAsync<int?>(detailSql, new { resolution.ComplaintHeaderId });
            if (itemIdResult.HasValue)
            {
                var items = await _itemLookup.GetByIdsAsync([itemIdResult.Value]);
                resolution.ItemName = items.Count > 0 ? items[0].ItemName : null;
            }

            var totalPacksSql = @"
                SELECT ISNULL(SUM(cd.NumberOfPacks), 0)
                FROM Sales.ComplaintDetail cd
                WHERE cd.ComplaintHeaderId = @ComplaintHeaderId AND cd.IsDeleted = 0;";
            resolution.ComplaintQuantity = await _dbConnection.ExecuteScalarAsync<decimal?>(totalPacksSql, new { resolution.ComplaintHeaderId });

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
