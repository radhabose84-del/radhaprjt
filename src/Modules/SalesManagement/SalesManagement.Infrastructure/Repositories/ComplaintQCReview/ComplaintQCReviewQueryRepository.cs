using System.Data;
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

        public ComplaintQCReviewQueryRepository(
            IDbConnection dbConnection,
            IUserLookup userLookup,
            IPartyLookup partyLookup)
        {
            _dbConnection = dbConnection;
            _userLookup = userLookup;
            _partyLookup = partyLookup;
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
                    ch.CustomerId
                FROM Sales.ComplaintQCReview r
                INNER JOIN Sales.ComplaintHeader ch ON r.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster pv ON r.PhysicalVerificationId = pv.Id AND pv.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster qcs ON r.ComplaintStatusId = qcs.Id AND qcs.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sev ON r.SeverityId = sev.Id AND sev.IsDeleted = 0
                WHERE r.IsDeleted = 0 {searchFilter} {statusFilterSql}
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
                // Get customer IDs from complaint headers
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

            var review = await _dbConnection.QueryFirstOrDefaultAsync<ComplaintQCReviewDto>(sql, parameters);

            if (review == null)
                return null;

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
            var customerIdSql = "SELECT CustomerId FROM Sales.ComplaintHeader WHERE Id = @Id AND IsDeleted = 0;";
            var customerId = await _dbConnection.ExecuteScalarAsync<int?>(customerIdSql, new { Id = review.ComplaintHeaderId });
            if (customerId.HasValue)
            {
                var party = await _partyLookup.GetByIdAsync(customerId.Value);
                review.CustomerName = party?.PartyName;
            }

            review.Assignments = assignments;

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
