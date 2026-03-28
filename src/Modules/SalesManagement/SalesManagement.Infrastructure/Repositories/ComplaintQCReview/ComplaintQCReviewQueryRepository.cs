using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.ComplaintQCReview.Dto;

namespace SalesManagement.Infrastructure.Repositories.ComplaintQCReview
{
    public class ComplaintQCReviewQueryRepository : IComplaintQCReviewQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUserLookup _userLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;

        public ComplaintQCReviewQueryRepository(
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

        public async Task<(List<QCReviewListDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, string? statusFilter)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : @" AND (ch.ComplaintNumber LIKE @SearchTerm OR ch.Remarks LIKE @SearchTerm)";

            var statusFilterSql = string.IsNullOrWhiteSpace(statusFilter)
                ? string.Empty
                : @" AND qcs.Description = @StatusFilter";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.ComplaintQCReview r
                INNER JOIN Sales.ComplaintHeader ch ON r.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster pv ON r.PhysicalVerificationId = pv.Id AND pv.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster qcs ON r.ComplaintStatusId = qcs.Id AND qcs.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sev ON r.SeverityId = sev.Id AND sev.IsDeleted = 0
                WHERE r.IsDeleted = 0 {searchFilter} {statusFilterSql};";

            var dataSql = $@"
                SELECT
                    r.Id,
                    r.ComplaintHeaderId,
                    ch.ComplaintNumber,
                    ch.ComplaintDate,
                    ch.CustomerId,
                    pv.Description AS PhysicalVerificationName,
                    qcs.Description AS ComplaintStatusName,
                    sev.Description AS SeverityName,
                    r.ReviewedBy,
                    r.ReviewedDate,
                    CASE
                        WHEN NOT EXISTS (
                            SELECT 1 FROM Sales.ComplaintQCReviewAssignment a WHERE a.ComplaintQCReviewId = r.Id AND a.IsDeleted = 0
                        ) THEN 'Pending'
                        WHEN COUNT(a2.Id) = SUM(
                            CASE WHEN LOWER(ast.Code) = 'submitted' THEN 1 ELSE 0 END
                        ) THEN 'All Feedback Received'
                        ELSE 'Pending'
                    END AS ReviewStatus
                FROM Sales.ComplaintQCReview r
                INNER JOIN Sales.ComplaintHeader ch ON r.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster pv ON r.PhysicalVerificationId = pv.Id AND pv.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster qcs ON r.ComplaintStatusId = qcs.Id AND qcs.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sev ON r.SeverityId = sev.Id AND sev.IsDeleted = 0
                LEFT JOIN Sales.ComplaintQCReviewAssignment a2 ON a2.ComplaintQCReviewId = r.Id AND a2.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ast ON a2.AssignmentStatusId = ast.Id AND ast.IsDeleted = 0
                WHERE r.IsDeleted = 0 {searchFilter} {statusFilterSql}
                GROUP BY r.Id, r.ComplaintHeaderId, ch.ComplaintNumber, ch.ComplaintDate, ch.CustomerId,
                    pv.Description, qcs.Description, sev.Description, r.ReviewedBy, r.ReviewedDate
                ORDER BY r.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                SearchTerm = $"%{searchTerm}%",
                StatusFilter = statusFilter,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<QCReviewListDto>(dataSql, parameters)).ToList();

            // Populate cross-module lookup names
            if (data.Count > 0)
            {
                // Populate CustomerName from party lookup
                var customerIdList = data.Where(d => d.CustomerId > 0).Select(d => d.CustomerId).Distinct().ToList();
                if (customerIdList.Count > 0)
                {
                    var parties = await _partyLookup.GetByIdsAsync(customerIdList);
                    var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                    foreach (var item in data.Where(d => d.CustomerId > 0))
                    {
                        if (partyDict.TryGetValue(item.CustomerId, out var custName))
                            item.CustomerName = custName;
                    }
                }

                // Populate ReviewedByName from user lookup
                var reviewerIds = data.Where(d => d.ReviewedBy.HasValue).Select(d => d.ReviewedBy!.Value).Distinct().ToList();
                if (reviewerIds.Count > 0)
                {
                    var users = await _userLookup.GetByIdsAsync(reviewerIds);
                    var userDict = users.ToDictionary(u => u.UserId, u => $"{u.FirstName} {u.LastName}".Trim());
                    foreach (var item in data.Where(d => d.ReviewedBy.HasValue))
                    {
                        if (userDict.TryGetValue(item.ReviewedBy!.Value, out var name))
                            item.ReviewedByName = name;
                    }
                }

                // Populate ItemName from first complaint detail via item lookup
                var complaintIds = data.Select(d => d.ComplaintHeaderId).Distinct().ToList();
                var itemSql = @"
                    SELECT cd.ComplaintHeaderId, cd.ItemId
                    FROM Sales.ComplaintDetail cd
                    WHERE cd.ComplaintHeaderId IN @Ids AND cd.IsDeleted = 0;";
                var detailItems = (await _dbConnection.QueryAsync<(int ComplaintHeaderId, int ItemId)>(
                    itemSql, new { Ids = complaintIds })).ToList();

                if (detailItems.Count > 0)
                {
                    var itemIds = detailItems.Select(d => d.ItemId).Distinct().ToList();
                    var items = await _itemLookup.GetByIdsAsync(itemIds);
                    var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                    // Use first item per complaint
                    var complaintItemDict = detailItems
                        .GroupBy(d => d.ComplaintHeaderId)
                        .ToDictionary(g => g.Key, g => g.First().ItemId);

                    foreach (var item in data)
                    {
                        if (complaintItemDict.TryGetValue(item.ComplaintHeaderId, out var itemId) &&
                            itemDict.TryGetValue(itemId, out var itemName))
                        {
                            item.ItemName = itemName;
                        }
                    }
                }
            }

            return (data, totalCount);
        }

        public async Task<ComplaintQCReviewDto?> GetByIdAsync(int id)
        {
            return await GetReviewAsync("r.Id = @Id", new { Id = id });
        }

        public async Task<ComplaintQCReviewDto?> GetByComplaintIdAsync(int complaintHeaderId)
        {
            return await GetReviewAsync("r.ComplaintHeaderId = @ComplaintHeaderId", new { ComplaintHeaderId = complaintHeaderId });
        }

        private async Task<ComplaintQCReviewDto?> GetReviewAsync(string whereClause, object parameters)
        {
            var sql = $@"
                SELECT
                    r.Id,
                    r.ComplaintHeaderId,
                    ch.ComplaintNumber,
                    ch.ComplaintDate,
                    ch.CustomerId,
                    r.PhysicalVerificationId,
                    pv.Description AS PhysicalVerificationName,
                    r.ComplaintStatusId,
                    qcs.Description AS ComplaintStatusName,
                    r.SeverityId,
                    sev.Description AS SeverityName,
                    r.CompensationStructureId,
                    comp.Description AS CompensationStructureName,
                    r.LabVerificationRequired,
                    r.LabResponsiblePersonId,
                    r.ExpectedResolutionDate,
                    r.Comments,
                    r.ReviewedBy,
                    r.ReviewedDate,
                    r.DecisionTimestamp,
                    r.IsActive,
                    r.IsDeleted
                FROM Sales.ComplaintQCReview r
                INNER JOIN Sales.ComplaintHeader ch ON r.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster pv ON r.PhysicalVerificationId = pv.Id AND pv.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster qcs ON r.ComplaintStatusId = qcs.Id AND qcs.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sev ON r.SeverityId = sev.Id AND sev.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster comp ON r.CompensationStructureId = comp.Id AND comp.IsDeleted = 0
                WHERE r.IsDeleted = 0 AND {whereClause};";

            // Use dynamic to capture CustomerId which is not on the DTO
            var rows = (await _dbConnection.QueryAsync(sql, parameters)).ToList();
            if (rows.Count == 0)
                return null;

            var row = rows[0];
            var review = new ComplaintQCReviewDto
            {
                Id = (int)row.Id,
                ComplaintHeaderId = (int)row.ComplaintHeaderId,
                ComplaintNumber = (string?)row.ComplaintNumber,
                ComplaintDate = row.ComplaintDate != null ? DateOnly.FromDateTime((DateTime)row.ComplaintDate) : null,
                PhysicalVerificationId = (int)row.PhysicalVerificationId,
                PhysicalVerificationName = (string?)row.PhysicalVerificationName,
                ComplaintStatusId = (int?)row.ComplaintStatusId,
                ComplaintStatusName = (string?)row.ComplaintStatusName,
                SeverityId = (int?)row.SeverityId,
                SeverityName = (string?)row.SeverityName,
                CompensationStructureId = (int?)row.CompensationStructureId,
                CompensationStructureName = (string?)row.CompensationStructureName,
                LabVerificationRequired = (bool)row.LabVerificationRequired,
                LabResponsiblePersonId = (int?)row.LabResponsiblePersonId,
                ExpectedResolutionDate = row.ExpectedResolutionDate != null ? DateOnly.FromDateTime((DateTime)row.ExpectedResolutionDate) : null,
                Comments = (string?)row.Comments,
                ReviewedBy = (int?)row.ReviewedBy,
                ReviewedDate = (DateTimeOffset?)row.ReviewedDate,
                DecisionTimestamp = (DateTimeOffset?)row.DecisionTimestamp,
                IsActive = (bool)row.IsActive,
                IsDeleted = (bool)row.IsDeleted
            };

            int? customerId = (int?)row.CustomerId;

            // Get assignments
            var assignmentSql = @"
                SELECT
                    a.Id,
                    a.ComplaintQCReviewId,
                    a.RoleId,
                    role.Description AS RoleName,
                    a.ResponsiblePersonId,
                    a.IsMandatory,
                    a.AssignmentStatusId,
                    ast.Description AS AssignmentStatusName
                FROM Sales.ComplaintQCReviewAssignment a
                LEFT JOIN Sales.MiscMaster role ON a.RoleId = role.Id AND role.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ast ON a.AssignmentStatusId = ast.Id AND ast.IsDeleted = 0
                WHERE a.ComplaintQCReviewId = @ReviewId AND a.IsDeleted = 0;";

            var assignments = (await _dbConnection.QueryAsync<ComplaintQCReviewAssignmentDto>(
                assignmentSql, new { ReviewId = review.Id })).ToList();

            // Populate cross-module user names
            var userIds = new List<int>();
            if (review.ReviewedBy.HasValue) userIds.Add(review.ReviewedBy.Value);
            if (review.LabResponsiblePersonId.HasValue) userIds.Add(review.LabResponsiblePersonId.Value);
            userIds.AddRange(assignments.Select(a => a.ResponsiblePersonId));

            if (userIds.Count > 0)
            {
                var users = await _userLookup.GetByIdsAsync(userIds.Distinct());
                var userDict = users.ToDictionary(u => u.UserId, u => $"{u.FirstName} {u.LastName}".Trim());

                if (review.ReviewedBy.HasValue && userDict.TryGetValue(review.ReviewedBy.Value, out var reviewerName))
                    review.ReviewedByName = reviewerName;

                if (review.LabResponsiblePersonId.HasValue && userDict.TryGetValue(review.LabResponsiblePersonId.Value, out var labName))
                    review.LabResponsiblePersonName = labName;

                foreach (var a in assignments)
                {
                    if (userDict.TryGetValue(a.ResponsiblePersonId, out var personName))
                        a.ResponsiblePersonName = personName;
                }
            }

            // Populate customer name
            if (customerId.HasValue)
            {
                var party = await _partyLookup.GetByIdAsync(customerId.Value);
                review.CustomerName = party?.PartyName;
            }

            // Populate item name from first complaint detail
            var itemSql = @"
                SELECT TOP 1 cd.ItemId
                FROM Sales.ComplaintDetail cd
                WHERE cd.ComplaintHeaderId = @ComplaintHeaderId AND cd.IsDeleted = 0;";
            var itemId = await _dbConnection.ExecuteScalarAsync<int?>(itemSql, new { review.ComplaintHeaderId });
            if (itemId.HasValue)
            {
                var items = await _itemLookup.GetByIdsAsync(new[] { itemId.Value });
                var item = items.FirstOrDefault();
                review.ItemName = item?.ItemName;
            }

            review.Assignments = assignments;

            // Derive ReviewStatus from assignment statuses
            if (assignments.Count == 0)
            {
                review.ReviewStatus = "Pending";
            }
            else
            {
                var allSubmitted = assignments.All(a =>
                    string.Equals(a.AssignmentStatusName, "Submitted", StringComparison.OrdinalIgnoreCase));
                review.ReviewStatus = allSubmitted ? "All Feedback Received" : "Pending";
            }

            return review;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.ComplaintQCReview WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> ComplaintExistsAsync(int complaintHeaderId)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.ComplaintHeader WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = complaintHeaderId });
            return count > 0;
        }

        public async Task<bool> ReviewAlreadyExistsAsync(int complaintHeaderId, int? excludeId = null)
        {
            var sql = excludeId.HasValue
                ? "SELECT COUNT(1) FROM Sales.ComplaintQCReview WHERE ComplaintHeaderId = @ComplaintHeaderId AND Id != @ExcludeId AND IsDeleted = 0;"
                : "SELECT COUNT(1) FROM Sales.ComplaintQCReview WHERE ComplaintHeaderId = @ComplaintHeaderId AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql,
                new { ComplaintHeaderId = complaintHeaderId, ExcludeId = excludeId ?? 0 });
            return count > 0;
        }

        public async Task<bool> MiscMasterExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.MiscMaster WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            var user = await _userLookup.GetByIdAsync(userId);
            return user != null;
        }
    }
}
