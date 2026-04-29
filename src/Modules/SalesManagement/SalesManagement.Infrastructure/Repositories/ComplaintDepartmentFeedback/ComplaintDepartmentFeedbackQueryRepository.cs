using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Domain.Common;

namespace SalesManagement.Infrastructure.Repositories.ComplaintDepartmentFeedback
{
    public class ComplaintDepartmentFeedbackQueryRepository : IComplaintDepartmentFeedbackQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUserLookup _userLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;
        private readonly ILotMasterLookup _lotLookup;

        public ComplaintDepartmentFeedbackQueryRepository(
            IDbConnection dbConnection,
            IUserLookup userLookup,
            IPartyLookup partyLookup,
            IItemLookup itemLookup,
            ILotMasterLookup lotLookup)
        {
            _dbConnection = dbConnection;
            _userLookup = userLookup;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
            _lotLookup = lotLookup;
        }

        public async Task<(List<FeedbackListDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, string? statusFilter, int? responsiblePersonId = null)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : @" AND (ch.ComplaintNumber LIKE @SearchTerm OR f.CorrectiveAction LIKE @SearchTerm OR f.RootCauseText LIKE @SearchTerm)";

            var personFilter = responsiblePersonId.HasValue
                ? " AND a.ResponsiblePersonId = @ResponsiblePersonId"
                : string.Empty;

            var statusCondition = string.Empty;
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                if (statusFilter.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    statusCondition = " AND f.Id IS NULL";
                else
                    statusCondition = " AND fs.Description = @StatusFilter";
            }

            // Only surface feedback whose parent QC Review has been approved
            // (ComplaintQCReview.ComplaintStatusId → Sales.MiscMaster.Code = 'QC Accepted').
            // Rejected or still-pending QC reviews are hidden from the list.
            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.ComplaintQCReviewAssignment a
                INNER JOIN Sales.ComplaintQCReview r ON a.ComplaintQCReviewId = r.Id AND r.IsDeleted = 0
                INNER JOIN Sales.MiscMaster qcrStatus ON r.ComplaintStatusId = qcrStatus.Id AND qcrStatus.IsDeleted = 0
                INNER JOIN Sales.ComplaintHeader ch ON r.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.ComplaintDepartmentFeedback f ON f.AssignmentId = a.Id AND f.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster fs ON f.FeedbackStatusId = fs.Id AND fs.IsDeleted = 0
                WHERE a.IsDeleted = 0
                    AND qcrStatus.Code = 'QC Accepted'
                    {searchFilter} {statusCondition} {personFilter};";

            var dataSql = $@"
                SELECT
                    f.Id AS FeedbackId,
                    a.Id AS AssignmentId,
                    ch.Id AS ComplaintHeaderId,
                    ch.ComplaintNumber,
                    ch.ComplaintDate,
                    ch.CustomerId,
                    role.Description AS RoleName,
                    a.ResponsiblePersonId,
                    CASE
                        WHEN f.Id IS NULL THEN 'Pending'
                        ELSE fs.Description
                    END AS FeedbackStatusName,
                    sev.Description AS SeverityName,
                    r.ExpectedResolutionDate,
                    f.SubmittedDate,
                    ISNULL(f.ReworkCount, 0) AS ReworkCount,
                    a.IsMandatory
                FROM Sales.ComplaintQCReviewAssignment a
                INNER JOIN Sales.ComplaintQCReview r ON a.ComplaintQCReviewId = r.Id AND r.IsDeleted = 0
                INNER JOIN Sales.MiscMaster qcrStatus ON r.ComplaintStatusId = qcrStatus.Id AND qcrStatus.IsDeleted = 0
                INNER JOIN Sales.ComplaintHeader ch ON r.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.ComplaintDepartmentFeedback f ON f.AssignmentId = a.Id AND f.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster role ON a.RoleId = role.Id AND role.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster fs ON f.FeedbackStatusId = fs.Id AND fs.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sev ON r.SeverityId = sev.Id AND sev.IsDeleted = 0
                WHERE a.IsDeleted = 0
                    AND qcrStatus.Code = 'QC Accepted'
                    {searchFilter} {statusCondition} {personFilter}
                ORDER BY a.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                SearchTerm = $"%{searchTerm}%",
                StatusFilter = statusFilter,
                ResponsiblePersonId = responsiblePersonId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<FeedbackListDto>(dataSql, parameters)).ToList();

            if (data.Count > 0)
            {
                await PopulateListLookupNames(data);
            }

            return (data, totalCount);
        }

        public async Task<ComplaintDepartmentFeedbackDto?> GetByIdAsync(int id)
        {
            return await GetFeedbackAsync("f.Id = @Id", new { Id = id });
        }

        public async Task<ComplaintDepartmentFeedbackDto?> GetByAssignmentIdAsync(int assignmentId)
        {
            const string sql = @"
                SELECT
                    f.Id,
                    a.Id AS AssignmentId,
                    r.ComplaintHeaderId,
                    ch.ComplaintNumber,
                    ch.ComplaintDate,
                    ch.CustomerId,
                    ch.Remarks AS ComplaintDescription,
                    sev.Description AS SeverityName,
                    r.ExpectedResolutionDate,
                    role.Description AS RoleName,
                    a.ResponsiblePersonId,
                    a.IsMandatory,
                    f.RootCauseText,
                    f.RootCauseCategoryId,
                    rcc.Description AS RootCauseCategoryName,
                    f.CorrectiveAction,
                    f.PreventiveAction,
                    f.Remarks,
                    ISNULL(f.FeedbackStatusId, 0) AS FeedbackStatusId,
                    CASE
                        WHEN f.Id IS NULL THEN 'Pending'
                        ELSE fs.Description
                    END AS FeedbackStatusName,
                    f.SubmittedBy,
                    f.SubmittedDate,
                    ISNULL(f.ReworkCount, 0) AS ReworkCount,
                    f.ReworkReason,
                    ISNULL(f.IsActive, 1) AS IsActive,
                    f.CreatedBy,
                    f.CreatedByName,
                    f.CreatedDate,
                    f.ModifiedBy,
                    f.ModifiedByName,
                    f.ModifiedDate
                FROM Sales.ComplaintQCReviewAssignment a
                INNER JOIN Sales.ComplaintQCReview r ON a.ComplaintQCReviewId = r.Id AND r.IsDeleted = 0
                INNER JOIN Sales.ComplaintHeader ch ON r.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.ComplaintDepartmentFeedback f ON f.AssignmentId = a.Id AND f.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster role ON a.RoleId = role.Id AND role.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster fs ON f.FeedbackStatusId = fs.Id AND fs.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sev ON r.SeverityId = sev.Id AND sev.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster rcc ON f.RootCauseCategoryId = rcc.Id AND rcc.IsDeleted = 0
                WHERE a.IsDeleted = 0 AND a.Id = @AssignmentId;";

            var rows = (await _dbConnection.QueryAsync(sql, new { AssignmentId = assignmentId })).ToList();
            if (rows.Count == 0)
                return null;

            var row = rows[0];
            var dto = new ComplaintDepartmentFeedbackDto
            {
                Id = (int?)row.Id ?? 0,
                AssignmentId = (int)row.AssignmentId,
                ComplaintHeaderId = (int)row.ComplaintHeaderId,
                ComplaintNumber = (string?)row.ComplaintNumber,
                ComplaintDate = row.ComplaintDate != null ? DateOnly.FromDateTime((DateTime)row.ComplaintDate) : null,
                CustomerId = (int)row.CustomerId,
                ComplaintDescription = (string?)row.ComplaintDescription,
                SeverityName = (string?)row.SeverityName,
                ExpectedResolutionDate = row.ExpectedResolutionDate != null ? DateOnly.FromDateTime((DateTime)row.ExpectedResolutionDate) : null,
                RoleName = (string?)row.RoleName,
                ResponsiblePersonId = (int)row.ResponsiblePersonId,
                IsMandatory = (bool)row.IsMandatory,
                RootCauseText = (string?)row.RootCauseText,
                RootCauseCategoryId = (int?)row.RootCauseCategoryId,
                RootCauseCategoryName = (string?)row.RootCauseCategoryName,
                CorrectiveAction = (string?)row.CorrectiveAction,
                PreventiveAction = (string?)row.PreventiveAction,
                Remarks = (string?)row.Remarks,
                FeedbackStatusId = (int)row.FeedbackStatusId,
                FeedbackStatusName = (string?)row.FeedbackStatusName,
                SubmittedBy = (int?)row.SubmittedBy,
                SubmittedDate = (DateTimeOffset?)row.SubmittedDate,
                ReworkCount = (int)row.ReworkCount,
                ReworkReason = (string?)row.ReworkReason,
                IsActive = (bool)row.IsActive,
                CreatedBy = (int?)row.CreatedBy ?? 0,
                CreatedByName = (string?)row.CreatedByName,
                CreatedDate = (DateTimeOffset?)row.CreatedDate,
                ModifiedBy = (int?)row.ModifiedBy,
                ModifiedByName = (string?)row.ModifiedByName,
                ModifiedDate = (DateTimeOffset?)row.ModifiedDate
            };

            // Populate cross-module user names
            var userIds = new List<int>();
            if (dto.SubmittedBy.HasValue) userIds.Add(dto.SubmittedBy.Value);
            userIds.Add(dto.ResponsiblePersonId);

            if (userIds.Count > 0)
            {
                var users = await _userLookup.GetByIdsAsync(userIds.Distinct());
                var userDict = users.ToDictionary(u => u.UserId, u => $"{u.FirstName} {u.LastName}".Trim());

                if (dto.SubmittedBy.HasValue && userDict.TryGetValue(dto.SubmittedBy.Value, out var submitterName))
                    dto.SubmittedByName = submitterName;

                if (userDict.TryGetValue(dto.ResponsiblePersonId, out var personName))
                    dto.ResponsiblePersonName = personName;
            }

            // Populate customer name
            if (dto.CustomerId > 0)
            {
                var party = await _partyLookup.GetByIdAsync(dto.CustomerId);
                dto.CustomerName = party?.PartyName;
            }

            // Populate ProductName, LotNo, PackDetails from first complaint detail
            const string detailSql = @"
                SELECT TOP 1 cd.ItemId, cd.LotId, cd.NumberOfPacks, cd.NetWeight
                FROM Sales.ComplaintDetail cd
                WHERE cd.ComplaintHeaderId = @HeaderId AND cd.IsDeleted = 0;";
            var detail = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(detailSql, new { HeaderId = dto.ComplaintHeaderId });

            if (detail != null)
            {
                int? itemId = (int?)detail.ItemId;
                int? lotId = (int?)detail.LotId;
                int? numberOfPacks = (int?)detail.NumberOfPacks;
                decimal? netWeight = (decimal?)detail.NetWeight;

                if (itemId.HasValue)
                {
                    var items = await _itemLookup.GetByIdsAsync(new[] { itemId.Value });
                    dto.ProductName = items.FirstOrDefault()?.ItemName;
                }

                if (lotId.HasValue)
                {
                    var lots = await _lotLookup.GetByIdsAsync(new[] { lotId.Value });
                    dto.LotNo = lots.FirstOrDefault()?.LotCode;
                }

                if (numberOfPacks.HasValue && netWeight.HasValue)
                    dto.PackDetails = $"{numberOfPacks.Value} Bags x {netWeight.Value} kg each";
                else if (numberOfPacks.HasValue)
                    dto.PackDetails = $"{numberOfPacks.Value} Bags";
            }

            // Get attachments if feedback exists
            if (dto.Id > 0)
            {
                const string attachmentSql = @"
                    SELECT Id, FeedbackId, FileName, FilePath, FileType, FileSize
                    FROM Sales.ComplaintFeedbackAttachment
                    WHERE FeedbackId = @FeedbackId AND IsDeleted = 0;";
                dto.Attachments = (await _dbConnection.QueryAsync<ComplaintFeedbackAttachmentDto>(
                    attachmentSql, new { FeedbackId = dto.Id })).ToList();
            }

            return dto;
        }

        public async Task<List<FeedbackListDto>> GetByComplaintIdAsync(int complaintHeaderId)
        {
            const string sql = @"
                SELECT
                    f.Id AS FeedbackId,
                    a.Id AS AssignmentId,
                    ch.Id AS ComplaintHeaderId,
                    ch.ComplaintNumber,
                    ch.ComplaintDate,
                    ch.CustomerId,
                    role.Description AS RoleName,
                    a.ResponsiblePersonId,
                    CASE
                        WHEN f.Id IS NULL THEN 'Pending'
                        ELSE fs.Description
                    END AS FeedbackStatusName,
                    sev.Description AS SeverityName,
                    r.ExpectedResolutionDate,
                    f.SubmittedDate,
                    ISNULL(f.ReworkCount, 0) AS ReworkCount,
                    a.IsMandatory
                FROM Sales.ComplaintQCReviewAssignment a
                INNER JOIN Sales.ComplaintQCReview r ON a.ComplaintQCReviewId = r.Id AND r.IsDeleted = 0
                INNER JOIN Sales.ComplaintHeader ch ON r.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.ComplaintDepartmentFeedback f ON f.AssignmentId = a.Id AND f.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster role ON a.RoleId = role.Id AND role.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster fs ON f.FeedbackStatusId = fs.Id AND fs.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sev ON r.SeverityId = sev.Id AND sev.IsDeleted = 0
                WHERE a.IsDeleted = 0 AND r.ComplaintHeaderId = @ComplaintHeaderId
                ORDER BY a.Id DESC;";

            var data = (await _dbConnection.QueryAsync<FeedbackListDto>(sql, new { ComplaintHeaderId = complaintHeaderId })).ToList();

            if (data.Count > 0)
            {
                await PopulateListLookupNames(data);
            }

            return data;
        }

        public async Task<List<MyPendingFeedbackDto>> GetMyPendingAsync(int userId)
        {
            const string sql = @"
                SELECT
                    a.Id AS AssignmentId,
                    f.Id AS FeedbackId,
                    ch.ComplaintNumber,
                    ch.CustomerId,
                    ch.Remarks AS ComplaintDescription,
                    sev.Description AS SeverityName,
                    r.ExpectedResolutionDate,
                    role.Description AS RoleName,
                    CASE
                        WHEN f.Id IS NULL THEN 'Pending'
                        ELSE fs.Description
                    END AS FeedbackStatus,
                    f.ReworkReason
                FROM Sales.ComplaintQCReviewAssignment a
                INNER JOIN Sales.ComplaintQCReview r ON a.ComplaintQCReviewId = r.Id AND r.IsDeleted = 0
                INNER JOIN Sales.ComplaintHeader ch ON r.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.ComplaintDepartmentFeedback f ON f.AssignmentId = a.Id AND f.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster role ON a.RoleId = role.Id AND role.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sev ON r.SeverityId = sev.Id AND sev.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster fs ON f.FeedbackStatusId = fs.Id AND fs.IsDeleted = 0
                WHERE a.IsDeleted = 0
                    AND a.ResponsiblePersonId = @UserId
                    AND (f.Id IS NULL OR fs.Description IN (@FeedbackPending, @FeedbackReworkRequired))
                    AND EXISTS (
                        SELECT 1 FROM Sales.MiscMaster hm
                        INNER JOIN Sales.MiscTypeMaster mt ON hm.MiscTypeId = mt.Id
                        WHERE hm.Id = ch.StatusId
                          AND mt.MiscTypeCode = @QCComplaintStatus
                          AND hm.IsDeleted = 0
                    )
                ORDER BY a.Id DESC;";

            var data = (await _dbConnection.QueryAsync<MyPendingFeedbackDto>(sql, new
            {
                UserId = userId,
                FeedbackPending = MiscEnumEntity.FeedbackPending,
                FeedbackReworkRequired = MiscEnumEntity.FeedbackReworkRequired,
                QCComplaintStatus = MiscEnumEntity.QCComplaintStatus
            })).ToList();

            if (data.Count > 0)
            {
                // Populate CustomerName
                var customerIds = data.Where(d => d.CustomerId > 0).Select(d => d.CustomerId).Distinct().ToList();
                if (customerIds.Count > 0)
                {
                    var parties = await _partyLookup.GetByIdsAsync(customerIds);
                    var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                    foreach (var item in data.Where(d => d.CustomerId > 0))
                    {
                        if (partyDict.TryGetValue(item.CustomerId, out var custName))
                            item.CustomerName = custName;
                    }
                }

                // Populate ProductName from first complaint detail
                var complaintNumbers = data.Select(d => d.ComplaintNumber).Distinct().ToList();
                var itemSql = @"
                    SELECT ch.ComplaintNumber, cd.ItemId
                    FROM Sales.ComplaintDetail cd
                    INNER JOIN Sales.ComplaintHeader ch ON cd.ComplaintHeaderId = ch.Id
                    WHERE ch.ComplaintNumber IN @Numbers AND cd.IsDeleted = 0;";
                var detailItems = (await _dbConnection.QueryAsync<(string ComplaintNumber, int ItemId)>(
                    itemSql, new { Numbers = complaintNumbers })).ToList();

                if (detailItems.Count > 0)
                {
                    var itemIds = detailItems.Select(d => d.ItemId).Distinct().ToList();
                    var items = await _itemLookup.GetByIdsAsync(itemIds);
                    var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                    var complaintItemDict = detailItems
                        .GroupBy(d => d.ComplaintNumber)
                        .ToDictionary(g => g.Key, g => g.First().ItemId);

                    foreach (var item in data)
                    {
                        if (item.ComplaintNumber != null &&
                            complaintItemDict.TryGetValue(item.ComplaintNumber, out var itemId) &&
                            itemDict.TryGetValue(itemId, out var itemName))
                        {
                            item.ProductName = itemName;
                        }
                    }
                }
            }

            return data;
        }

        public async Task<string?> GetAttachmentFilePathAsync(int id)
        {
            const string sql = @"
                SELECT FilePath FROM Sales.ComplaintFeedbackAttachment
                WHERE Id = @Id AND IsDeleted = 0;";
            return await _dbConnection.ExecuteScalarAsync<string?>(sql, new { Id = id });
        }

        // -------------------------------------------------------------------------
        // Per-complaint full-content view for QC.
        // Returns one row per assigned department with feedback content + attachments.
        // -------------------------------------------------------------------------

        public async Task<List<ComplaintFeedbackFullDto>> GetByComplaintIdWithContentAsync(int complaintHeaderId)
        {
            // One row per assignment for the complaint, with full feedback content (or nulls
            // when the assignment hasn't been submitted yet). Mirrors the JOIN pattern of
            // GetByComplaintIdAsync, but returns the heavy DTO with RCA + Remarks + audit fields.
            const string sql = @"
                SELECT
                    a.Id                            AS AssignmentId,
                    a.RoleId                        AS RoleId,
                    role.Description                AS RoleName,
                    a.ResponsiblePersonId           AS ResponsiblePersonId,
                    a.IsMandatory                   AS IsMandatory,
                    asn.Description                 AS AssignmentStatusName,
                    f.Id                            AS FeedbackId,
                    CASE
                        WHEN f.Id IS NULL THEN 'Pending'
                        ELSE fs.Description
                    END                             AS FeedbackStatusName,
                    f.RootCauseText                 AS RootCauseText,
                    f.RootCauseCategoryId           AS RootCauseCategoryId,
                    rcc.Description                 AS RootCauseCategoryName,
                    f.CorrectiveAction              AS CorrectiveAction,
                    f.PreventiveAction              AS PreventiveAction,
                    f.Remarks                       AS Remarks,
                    f.SubmittedBy                   AS SubmittedBy,
                    f.SubmittedDate                 AS SubmittedDate,
                    ISNULL(f.ReworkCount, 0)        AS ReworkCount,
                    f.ReworkReason                  AS ReworkReason
                FROM Sales.ComplaintQCReviewAssignment a
                INNER JOIN Sales.ComplaintQCReview r
                    ON a.ComplaintQCReviewId = r.Id AND r.IsDeleted = 0
                LEFT JOIN Sales.ComplaintDepartmentFeedback f
                    ON f.AssignmentId = a.Id AND f.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster role ON a.RoleId = role.Id AND role.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster asn  ON a.AssignmentStatusId = asn.Id AND asn.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster fs   ON f.FeedbackStatusId = fs.Id AND fs.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster rcc  ON f.RootCauseCategoryId = rcc.Id AND rcc.IsDeleted = 0
                WHERE a.IsDeleted = 0 AND r.ComplaintHeaderId = @ComplaintHeaderId
                ORDER BY a.IsMandatory DESC, a.Id ASC;";

            var rows = (await _dbConnection.QueryAsync<ComplaintFeedbackFullDto>(
                sql, new { ComplaintHeaderId = complaintHeaderId })).ToList();

            if (rows.Count == 0)
                return rows;

            // Cross-module name enrichment for ResponsiblePerson + SubmittedBy
            var userIds = new List<int>();
            userIds.AddRange(rows.Select(r => r.ResponsiblePersonId).Where(id => id > 0));
            userIds.AddRange(rows.Where(r => r.SubmittedBy.HasValue).Select(r => r.SubmittedBy!.Value));
            userIds = userIds.Distinct().ToList();

            if (userIds.Count > 0)
            {
                var users = await _userLookup.GetByIdsAsync(userIds);
                var userDict = users.ToDictionary(u => u.UserId, u => $"{u.FirstName} {u.LastName}".Trim());
                foreach (var row in rows)
                {
                    if (userDict.TryGetValue(row.ResponsiblePersonId, out var rpName))
                        row.ResponsiblePersonName = rpName;
                    if (row.SubmittedBy.HasValue && userDict.TryGetValue(row.SubmittedBy.Value, out var sbName))
                        row.SubmittedByName = sbName;
                }
            }

            // Fetch attachments per submitted feedback in one batch
            var feedbackIds = rows.Where(r => r.FeedbackId.HasValue)
                                  .Select(r => r.FeedbackId!.Value)
                                  .ToList();
            if (feedbackIds.Count > 0)
            {
                const string attachmentSql = @"
                    SELECT
                        Id, FeedbackId,
                        FileName, FilePath, FileType, FileSize
                    FROM Sales.ComplaintFeedbackAttachment
                    WHERE FeedbackId IN @Ids AND IsDeleted = 0
                    ORDER BY Id ASC;";

                var attachments = (await _dbConnection.QueryAsync<ComplaintFeedbackAttachmentDto>(
                    attachmentSql, new { Ids = feedbackIds })).ToList();

                var byFeedback = attachments.GroupBy(a => a.FeedbackId)
                                            .ToDictionary(g => g.Key, g => g.ToList());
                foreach (var row in rows)
                {
                    if (row.FeedbackId.HasValue && byFeedback.TryGetValue(row.FeedbackId.Value, out var list))
                        row.Attachments = list;
                }
            }

            return rows;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.ComplaintDepartmentFeedback WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> AssignmentExistsAsync(int assignmentId)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.ComplaintQCReviewAssignment WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = assignmentId });
            return count > 0;
        }

        public async Task<bool> IsQCApprovedForAssignmentAsync(int assignmentId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.ComplaintQCReviewAssignment a
                    INNER JOIN Sales.ComplaintQCReview qr ON a.ComplaintQCReviewId = qr.Id AND qr.IsDeleted = 0
                    INNER JOIN Sales.ComplaintHeader ch ON qr.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                    INNER JOIN Sales.MiscMaster hm ON ch.StatusId = hm.Id AND hm.IsDeleted = 0
                    INNER JOIN Sales.MiscTypeMaster mt ON hm.MiscTypeId = mt.Id
                    WHERE a.Id = @AssignmentId
                      AND a.IsDeleted = 0
                      AND mt.MiscTypeCode = @QCComplaintStatus
                ) THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new
            {
                AssignmentId = assignmentId,
                QCComplaintStatus = MiscEnumEntity.QCComplaintStatus
            });
        }

        public async Task<bool> IsQCApprovedForFeedbackAsync(int feedbackId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.ComplaintDepartmentFeedback f
                    INNER JOIN Sales.ComplaintQCReviewAssignment a ON f.AssignmentId = a.Id AND a.IsDeleted = 0
                    INNER JOIN Sales.ComplaintQCReview qr ON a.ComplaintQCReviewId = qr.Id AND qr.IsDeleted = 0
                    INNER JOIN Sales.ComplaintHeader ch ON qr.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                    INNER JOIN Sales.MiscMaster hm ON ch.StatusId = hm.Id AND hm.IsDeleted = 0
                    INNER JOIN Sales.MiscTypeMaster mt ON hm.MiscTypeId = mt.Id
                    WHERE f.Id = @FeedbackId
                      AND f.IsDeleted = 0
                      AND mt.MiscTypeCode = @QCComplaintStatus
                ) THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new
            {
                FeedbackId = feedbackId,
                QCComplaintStatus = MiscEnumEntity.QCComplaintStatus
            });
        }

        public async Task<bool> FeedbackAlreadyExistsForAssignmentAsync(int assignmentId)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.ComplaintDepartmentFeedback WHERE AssignmentId = @AssignmentId AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { AssignmentId = assignmentId });
            return count > 0;
        }

        public async Task<int> GetResponsiblePersonIdAsync(int assignmentId)
        {
            const string sql = "SELECT ResponsiblePersonId FROM Sales.ComplaintQCReviewAssignment WHERE Id = @Id AND IsDeleted = 0;";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = assignmentId });
        }

        public async Task<int> GetFeedbackStatusIdAsync(int feedbackId)
        {
            const string sql = "SELECT FeedbackStatusId FROM Sales.ComplaintDepartmentFeedback WHERE Id = @Id AND IsDeleted = 0;";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = feedbackId });
        }

        public async Task<int> GetAssignmentIdByFeedbackIdAsync(int feedbackId)
        {
            const string sql = "SELECT AssignmentId FROM Sales.ComplaintDepartmentFeedback WHERE Id = @Id AND IsDeleted = 0;";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = feedbackId });
        }

        public async Task<bool> MiscMasterExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.MiscMaster WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<(int ReworkCount, int FeedbackStatusId)> GetReworkInfoAsync(int feedbackId)
        {
            const string sql = "SELECT ReworkCount, FeedbackStatusId FROM Sales.ComplaintDepartmentFeedback WHERE Id = @Id AND IsDeleted = 0;";
            var result = await _dbConnection.QueryFirstOrDefaultAsync<(int ReworkCount, int FeedbackStatusId)?>(sql, new { Id = feedbackId });
            return result ?? (0, 0);
        }

        private async Task<ComplaintDepartmentFeedbackDto?> GetFeedbackAsync(string whereClause, object parameters)
        {
            var sql = $@"
                SELECT
                    f.Id,
                    f.AssignmentId,
                    r.ComplaintHeaderId,
                    ch.ComplaintNumber,
                    ch.ComplaintDate,
                    ch.CustomerId,
                    ch.Remarks AS ComplaintDescription,
                    sev.Description AS SeverityName,
                    r.ExpectedResolutionDate,
                    role.Description AS RoleName,
                    a.ResponsiblePersonId,
                    a.IsMandatory,
                    f.RootCauseText,
                    f.RootCauseCategoryId,
                    rcc.Description AS RootCauseCategoryName,
                    f.CorrectiveAction,
                    f.PreventiveAction,
                    f.Remarks,
                    f.FeedbackStatusId,
                    fs.Description AS FeedbackStatusName,
                    f.SubmittedBy,
                    f.SubmittedDate,
                    f.ReworkCount,
                    f.ReworkReason,
                    f.IsActive,
                    f.CreatedBy,
                    f.CreatedByName,
                    f.CreatedDate,
                    f.ModifiedBy,
                    f.ModifiedByName,
                    f.ModifiedDate
                FROM Sales.ComplaintDepartmentFeedback f
                INNER JOIN Sales.ComplaintQCReviewAssignment a ON f.AssignmentId = a.Id AND a.IsDeleted = 0
                INNER JOIN Sales.ComplaintQCReview r ON a.ComplaintQCReviewId = r.Id AND r.IsDeleted = 0
                INNER JOIN Sales.ComplaintHeader ch ON r.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster role ON a.RoleId = role.Id AND role.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster fs ON f.FeedbackStatusId = fs.Id AND fs.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sev ON r.SeverityId = sev.Id AND sev.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster rcc ON f.RootCauseCategoryId = rcc.Id AND rcc.IsDeleted = 0
                WHERE f.IsDeleted = 0 AND {whereClause};";

            var rows = (await _dbConnection.QueryAsync(sql, parameters)).ToList();
            if (rows.Count == 0)
                return null;

            var row = rows[0];
            var dto = new ComplaintDepartmentFeedbackDto
            {
                Id = (int)row.Id,
                AssignmentId = (int)row.AssignmentId,
                ComplaintHeaderId = (int)row.ComplaintHeaderId,
                ComplaintNumber = (string?)row.ComplaintNumber,
                ComplaintDate = row.ComplaintDate != null ? DateOnly.FromDateTime((DateTime)row.ComplaintDate) : null,
                CustomerId = (int)row.CustomerId,
                ComplaintDescription = (string?)row.ComplaintDescription,
                SeverityName = (string?)row.SeverityName,
                ExpectedResolutionDate = row.ExpectedResolutionDate != null ? DateOnly.FromDateTime((DateTime)row.ExpectedResolutionDate) : null,
                RoleName = (string?)row.RoleName,
                ResponsiblePersonId = (int)row.ResponsiblePersonId,
                IsMandatory = (bool)row.IsMandatory,
                RootCauseText = (string?)row.RootCauseText,
                RootCauseCategoryId = (int?)row.RootCauseCategoryId,
                RootCauseCategoryName = (string?)row.RootCauseCategoryName,
                CorrectiveAction = (string?)row.CorrectiveAction,
                PreventiveAction = (string?)row.PreventiveAction,
                Remarks = (string?)row.Remarks,
                FeedbackStatusId = (int)row.FeedbackStatusId,
                FeedbackStatusName = (string?)row.FeedbackStatusName,
                SubmittedBy = (int?)row.SubmittedBy,
                SubmittedDate = (DateTimeOffset?)row.SubmittedDate,
                ReworkCount = (int)row.ReworkCount,
                ReworkReason = (string?)row.ReworkReason,
                IsActive = (bool)row.IsActive,
                CreatedBy = (int)row.CreatedBy,
                CreatedByName = (string?)row.CreatedByName,
                CreatedDate = (DateTimeOffset?)row.CreatedDate,
                ModifiedBy = (int?)row.ModifiedBy,
                ModifiedByName = (string?)row.ModifiedByName,
                ModifiedDate = (DateTimeOffset?)row.ModifiedDate
            };

            // Populate cross-module user names
            var userIds = new List<int>();
            if (dto.SubmittedBy.HasValue) userIds.Add(dto.SubmittedBy.Value);
            userIds.Add(dto.ResponsiblePersonId);

            if (userIds.Count > 0)
            {
                var users = await _userLookup.GetByIdsAsync(userIds.Distinct());
                var userDict = users.ToDictionary(u => u.UserId, u => $"{u.FirstName} {u.LastName}".Trim());

                if (dto.SubmittedBy.HasValue && userDict.TryGetValue(dto.SubmittedBy.Value, out var submitterName))
                    dto.SubmittedByName = submitterName;

                if (userDict.TryGetValue(dto.ResponsiblePersonId, out var personName))
                    dto.ResponsiblePersonName = personName;
            }

            // Populate customer name
            if (dto.CustomerId > 0)
            {
                var party = await _partyLookup.GetByIdAsync(dto.CustomerId);
                dto.CustomerName = party?.PartyName;
            }

            // Populate ProductName, LotNo, PackDetails from first complaint detail
            const string detailSql = @"
                SELECT TOP 1 cd.ItemId, cd.LotId, cd.NumberOfPacks, cd.NetWeight
                FROM Sales.ComplaintDetail cd
                WHERE cd.ComplaintHeaderId = @HeaderId AND cd.IsDeleted = 0;";
            var detail = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(detailSql, new { HeaderId = dto.ComplaintHeaderId });

            if (detail != null)
            {
                int? itemId = (int?)detail.ItemId;
                int? lotId = (int?)detail.LotId;
                int? numberOfPacks = (int?)detail.NumberOfPacks;
                decimal? netWeight = (decimal?)detail.NetWeight;

                if (itemId.HasValue)
                {
                    var items = await _itemLookup.GetByIdsAsync(new[] { itemId.Value });
                    dto.ProductName = items.FirstOrDefault()?.ItemName;
                }

                if (lotId.HasValue)
                {
                    var lots = await _lotLookup.GetByIdsAsync(new[] { lotId.Value });
                    dto.LotNo = lots.FirstOrDefault()?.LotCode;
                }

                if (numberOfPacks.HasValue && netWeight.HasValue)
                    dto.PackDetails = $"{numberOfPacks.Value} Bags x {netWeight.Value} kg each";
                else if (numberOfPacks.HasValue)
                    dto.PackDetails = $"{numberOfPacks.Value} Bags";
            }

            // Get attachments
            const string attachmentSql = @"
                SELECT Id, FeedbackId, FileName, FilePath, FileType, FileSize
                FROM Sales.ComplaintFeedbackAttachment
                WHERE FeedbackId = @FeedbackId AND IsDeleted = 0;";

            dto.Attachments = (await _dbConnection.QueryAsync<ComplaintFeedbackAttachmentDto>(
                attachmentSql, new { FeedbackId = dto.Id })).ToList();

            return dto;
        }

        private async Task PopulateListLookupNames(List<FeedbackListDto> data)
        {
            // Populate CustomerName from party lookup
            var customerIds = data.Where(d => d.CustomerId > 0).Select(d => d.CustomerId).Distinct().ToList();
            if (customerIds.Count > 0)
            {
                var parties = await _partyLookup.GetByIdsAsync(customerIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                foreach (var item in data.Where(d => d.CustomerId > 0))
                {
                    if (partyDict.TryGetValue(item.CustomerId, out var custName))
                        item.CustomerName = custName;
                }
            }

            // Populate ResponsiblePersonName from user lookup
            var personIds = data.Select(d => d.ResponsiblePersonId).Distinct().ToList();
            if (personIds.Count > 0)
            {
                var users = await _userLookup.GetByIdsAsync(personIds);
                var userDict = users.ToDictionary(u => u.UserId, u => $"{u.FirstName} {u.LastName}".Trim());
                foreach (var item in data)
                {
                    if (userDict.TryGetValue(item.ResponsiblePersonId, out var name))
                        item.ResponsiblePersonName = name;
                }
            }

            // Populate ProductName from first complaint detail item
            var complaintNumbers = data.Where(d => d.ComplaintNumber != null).Select(d => d.ComplaintNumber).Distinct().ToList();
            if (complaintNumbers.Count > 0)
            {
                var itemSql = @"
                    SELECT ch.ComplaintNumber, cd.ItemId
                    FROM Sales.ComplaintDetail cd
                    INNER JOIN Sales.ComplaintHeader ch ON cd.ComplaintHeaderId = ch.Id
                    WHERE ch.ComplaintNumber IN @Numbers AND cd.IsDeleted = 0;";
                var detailItems = (await _dbConnection.QueryAsync<(string ComplaintNumber, int ItemId)>(
                    itemSql, new { Numbers = complaintNumbers })).ToList();

                if (detailItems.Count > 0)
                {
                    var itemIds = detailItems.Select(d => d.ItemId).Distinct().ToList();
                    var items = await _itemLookup.GetByIdsAsync(itemIds);
                    var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

                    var complaintItemDict = detailItems
                        .GroupBy(d => d.ComplaintNumber)
                        .ToDictionary(g => g.Key!, g => g.First().ItemId);

                    foreach (var item in data)
                    {
                        if (item.ComplaintNumber != null &&
                            complaintItemDict.TryGetValue(item.ComplaintNumber, out var itemId) &&
                            itemDict.TryGetValue(itemId, out var itemName))
                        {
                            item.ProductName = itemName;
                        }
                    }
                }
            }
        }
    }
}
