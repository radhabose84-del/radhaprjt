using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Domain.Common;

namespace SalesManagement.Infrastructure.Repositories.Complaint
{
    public class ComplaintQueryRepository : IComplaintQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;
        private readonly ILotMasterLookup _lotLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IDataAccessFilter _dataAccessFilter;

        public ComplaintQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IItemLookup itemLookup,
            ILotMasterLookup lotLookup,
            IUnitLookup unitLookup,
            IUOMLookup uomLookup,
            IIPAddressService ipAddressService,
            IDataAccessFilter dataAccessFilter)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
            _lotLookup = lotLookup;
            _unitLookup = unitLookup;
            _uomLookup = uomLookup;
            _ipAddressService = ipAddressService;
            _dataAccessFilter = dataAccessFilter;
        }

        public async Task<(List<ComplaintHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, string? statusFilter)
        {
            var accessCtx = await _dataAccessFilter.GetContextAsync();
            var allowedPartyIds = new HashSet<int>(accessCtx.AllowedCustomerIds);

            if (accessCtx.IsCustomerRestricted && allowedPartyIds.Count == 0)
                return (new List<ComplaintHeaderDto>(), 0);

            var partyFilter = accessCtx.IsCustomerRestricted && allowedPartyIds.Count > 0
                ? " AND h.CustomerId IN @AllowedPartyIds"
                : string.Empty;

            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : @" AND (h.ComplaintNumber LIKE @SearchTerm
                       OR ms.Description LIKE @SearchTerm
                       OR h.Remarks LIKE @SearchTerm)";

            var statusFilterSql = string.IsNullOrWhiteSpace(statusFilter)
                ? string.Empty
                : " AND ms.Description = @StatusFilter";

            var allFilters = $"{partyFilter}{searchFilter}{statusFilterSql}";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.ComplaintHeader h
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.IsDeleted = 0 {allFilters};";

            var dataSql = $@"
                SELECT
                    h.Id,
                    h.ComplaintNumber,
                    h.ComplaintDate,
                    h.CustomerId,
                    h.CustomerAddress,
                    h.CustomerPIN,
                    h.CustomerMobile,
                    h.CustomerEmail,
                    h.CustomerPAN,
                    h.CustomerGSTNo,
                    h.CreditLimit,
                    h.TotalOS,
                    h.Outstanding,
                    h.BalanceCredit,
                    h.Delay,
                    h.Ledger,
                    h.StatusId,
                    ms.Description AS StatusName,
                    h.Remarks,
                    h.IsActive,
                    h.IsDeleted,
                    h.CreatedBy,
                    h.CreatedDate,
                    h.CreatedByName,
                    h.CreatedIP,
                    h.ModifiedBy,
                    h.ModifiedDate,
                    h.ModifiedByName,
                    h.ModifiedIP
                FROM Sales.ComplaintHeader h
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.IsDeleted = 0 {allFilters}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var dp = new DynamicParameters();
            dp.Add("SearchTerm", $"%{searchTerm}%");
            dp.Add("StatusFilter", statusFilter);
            dp.Add("Offset", (pageNumber - 1) * pageSize);
            dp.Add("PageSize", pageSize);
            if (accessCtx.IsCustomerRestricted && allowedPartyIds.Count > 0)
                dp.Add("AllowedPartyIds", allowedPartyIds.ToList());

            using var multi = await _dbConnection.QueryMultipleAsync(countSql + dataSql, dp);
            var totalCount = await multi.ReadFirstAsync<int>();
            var data = (await multi.ReadAsync<ComplaintHeaderDto>()).ToList();

            // Populate CustomerName via cross-module lookup
            if (data.Count > 0)
            {
                var customerIds = data.Select(d => d.CustomerId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(customerIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                foreach (var item in data)
                {
                    item.CustomerName = partyDict.TryGetValue(item.CustomerId, out var name) ? name : null;
                }

                // Derive ComplaintStage for each complaint
                foreach (var item in data)
                {
                    item.ComplaintStage = await DeriveComplaintStageAsync(item.Id);
                }
            }

            return (data, totalCount);
        }

        public async Task<ComplaintHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT
                    h.Id,
                    h.ComplaintNumber,
                    h.ComplaintDate,
                    h.CustomerId,
                    h.CustomerAddress,
                    h.CustomerPIN,
                    h.CustomerMobile,
                    h.CustomerEmail,
                    h.CustomerPAN,
                    h.CustomerGSTNo,
                    h.CreditLimit,
                    h.TotalOS,
                    h.Outstanding,
                    h.BalanceCredit,
                    h.Delay,
                    h.Ledger,
                    h.StatusId,
                    ms.Description AS StatusName,
                    h.Remarks,
                    h.IsActive,
                    h.IsDeleted,
                    h.CreatedBy,
                    h.CreatedDate,
                    h.CreatedByName,
                    h.CreatedIP,
                    h.ModifiedBy,
                    h.ModifiedDate,
                    h.ModifiedByName,
                    h.ModifiedIP
                FROM Sales.ComplaintHeader h
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0;";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<ComplaintHeaderDto>(headerSql, new { Id = id });

            if (header == null)
                return null;

            // Populate CustomerName
            var party = await _partyLookup.GetByIdAsync(header.CustomerId);
            header.CustomerName = party?.PartyName;

            // Fetch details with same-module JOINs only
            const string detailSql = @"
                SELECT
                    d.Id,
                    d.ComplaintHeaderId,
                    d.InvoiceHeaderId,
                    ih.InvoiceNo,
                    d.InvoiceDate,
                    d.InvoiceTypeId,
                    mt.Description AS InvoiceTypeName,
                    d.LotId,
                    d.ItemId,
                    d.NumberOfPacks,
                    d.NetWeight,
                    d.InvoiceAmount,
                    d.DivisionId
                FROM Sales.ComplaintDetail d
                LEFT JOIN Sales.InvoiceHeader ih ON d.InvoiceHeaderId = ih.Id AND ih.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mt ON d.InvoiceTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE d.ComplaintHeaderId = @HeaderId AND d.IsDeleted = 0;";

            var details = (await _dbConnection.QueryAsync<ComplaintDetailDto>(detailSql, new { HeaderId = id })).ToList();

            // Populate cross-module lookups for details
            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

                var lotIds = details.Where(d => d.LotId.HasValue).Select(d => d.LotId!.Value).Distinct();
                var lots = await _lotLookup.GetByIdsAsync(lotIds);
                var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);

                var divUnitIds = details.Where(d => d.DivisionId.HasValue).Select(d => d.DivisionId!.Value).Distinct();
                var divUnits = await _unitLookup.GetByIdsAsync(divUnitIds);
                var divisionDict = divUnits.ToDictionary(u => u.UnitId, u => u.UnitName);

                foreach (var detail in details)
                {
                    if (itemDict.TryGetValue(detail.ItemId, out var itemInfo))
                    {
                        detail.ItemCode = itemInfo.ItemCode;
                        detail.ItemName = itemInfo.ItemName;
                    }
                    detail.LotCode = detail.LotId.HasValue && lotDict.TryGetValue(detail.LotId.Value, out var lotCode) ? lotCode : null;
                    detail.DivisionName = detail.DivisionId.HasValue && divisionDict.TryGetValue(detail.DivisionId.Value, out var divName) ? divName : null;
                }

                // Fetch nature of complaints for all details
                var detailIds = details.Select(d => d.Id).ToList();
                const string natureSql = @"
                    SELECT
                        cn.Id,
                        cn.ComplaintDetailId,
                        cn.NatureOfComplaintId,
                        mm.Description AS NatureOfComplaintName
                    FROM Sales.ComplaintDetailNature cn
                    LEFT JOIN Sales.MiscMaster mm ON cn.NatureOfComplaintId = mm.Id AND mm.IsDeleted = 0
                    WHERE cn.ComplaintDetailId IN @DetailIds;";

                var natures = (await _dbConnection.QueryAsync<ComplaintDetailNatureDto>(natureSql, new { DetailIds = detailIds })).ToList();

                foreach (var detail in details)
                {
                    detail.NatureOfComplaints = natures.Where(n => n.ComplaintDetailId == detail.Id).ToList();
                }
            }

            header.ComplaintDetails = details;

            // Fetch attachments
            const string attachmentSql = @"
                SELECT Id, ComplaintHeaderId, FileName, FilePath, FileType, FileSize
                FROM Sales.ComplaintAttachment
                WHERE ComplaintHeaderId = @HeaderId AND IsDeleted = 0;";
            header.Attachments = [.. await _dbConnection.QueryAsync<ComplaintAttachmentDto>(
                attachmentSql, new { HeaderId = id })];

            // Derive ComplaintStage from related tables
            header.ComplaintStage = await DeriveComplaintStageAsync(header.Id);

            return header;
        }

        private async Task<string> DeriveComplaintStageAsync(int complaintHeaderId)
        {
            const string sql = @"
                SELECT
                    qr.Id AS QCReviewId,
                    qcs.Code AS QCDecision,
                    cr.Id AS ResolutionId,
                    cs.Code AS ClosureStatusCode,
                    (SELECT COUNT(*) FROM Sales.ComplaintQCReviewAssignment a
                     WHERE a.ComplaintQCReviewId = qr.Id AND a.IsDeleted = 0) AS TotalAssignments,
                    (SELECT COUNT(*) FROM Sales.ComplaintQCReviewAssignment a
                     INNER JOIN Sales.MiscMaster ast ON a.AssignmentStatusId = ast.Id
                     WHERE a.ComplaintQCReviewId = qr.Id AND a.IsDeleted = 0
                     AND LOWER(ast.Code) = 'submitted') AS SubmittedAssignments
                FROM Sales.ComplaintHeader ch
                LEFT JOIN Sales.ComplaintQCReview qr ON qr.ComplaintHeaderId = ch.Id AND qr.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster qcs ON qr.ComplaintStatusId = qcs.Id AND qcs.IsDeleted = 0
                LEFT JOIN Sales.ComplaintResolution cr ON cr.ComplaintHeaderId = ch.Id AND cr.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cs ON cr.ClosureStatusId = cs.Id AND cs.IsDeleted = 0
                WHERE ch.Id = @ComplaintHeaderId AND ch.IsDeleted = 0;";

            var row = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(sql, new { ComplaintHeaderId = complaintHeaderId });

            if (row == null)
                return "Registered";

            int? qcReviewId = (int?)row.QCReviewId;
            string? qcDecision = (string?)row.QCDecision;
            int? resolutionId = (int?)row.ResolutionId;
            string? closureStatus = (string?)row.ClosureStatusCode;
            int totalAssignments = (int?)row.TotalAssignments ?? 0;
            int submittedAssignments = (int?)row.SubmittedAssignments ?? 0;

            // Resolution + Closure
            if (resolutionId.HasValue)
            {
                return closureStatus?.ToLower() switch
                {
                    "closed" => "Closed",
                    "ready for closure" => "Ready for Closure",
                    _ => "Resolution In Progress"
                };
            }

            // QC Review exists
            if (qcReviewId.HasValue)
            {
                if (string.Equals(qcDecision, "QC Rejected", StringComparison.OrdinalIgnoreCase))
                    return "QC Rejected";

                // QC Accepted — check feedback
                if (totalAssignments > 0 && totalAssignments == submittedAssignments)
                    return "Feedback Completed";

                if (totalAssignments > 0)
                    return "Awaiting Department Feedback";

                return "Under QC Review";
            }

            return "Pending QC Review";
        }

        public async Task<IReadOnlyList<ComplaintLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            var accessCtx = await _dataAccessFilter.GetContextAsync(ct);
            var allowedPartyIds = new HashSet<int>(accessCtx.AllowedCustomerIds);
            if (accessCtx.PartyId.HasValue)
                allowedPartyIds.Add(accessCtx.PartyId.Value);

            if (accessCtx.IsCustomerRestricted && allowedPartyIds.Count == 0)
                return new List<ComplaintLookupDto>();

            var partyFilter = accessCtx.IsCustomerRestricted && allowedPartyIds.Count > 0
                ? " AND h.CustomerId IN @AllowedPartyIds"
                : string.Empty;

            var sql = $@"
                SELECT TOP 20
                    h.Id,
                    h.ComplaintNumber,
                    h.ComplaintDate
                FROM Sales.ComplaintHeader h
                WHERE h.IsDeleted = 0 AND h.IsActive = 1
                  AND h.ComplaintNumber LIKE @Term
                  {partyFilter}
                ORDER BY h.ComplaintNumber ASC;";

            var dp = new DynamicParameters();
            dp.Add("Term", $"%{term}%");
            if (accessCtx.IsCustomerRestricted && allowedPartyIds.Count > 0)
                dp.Add("AllowedPartyIds", allowedPartyIds.ToList());

            var result = await _dbConnection.QueryAsync<ComplaintLookupDto>(sql, dp);
            var list = result.ToList();

            // Populate CustomerName
            if (list.Count > 0)
            {
                // Need to fetch CustomerIds - run a quick query
                const string customerSql = @"
                    SELECT Id, CustomerId FROM Sales.ComplaintHeader WHERE Id IN @Ids;";
                var customerMap = (await _dbConnection.QueryAsync<(int Id, int CustomerId)>(customerSql, new { Ids = list.Select(l => l.Id).ToList() }))
                    .ToDictionary(x => x.Id, x => x.CustomerId);

                var customerIds = customerMap.Values.Distinct();
                var parties = await _partyLookup.GetByIdsAsync(customerIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                foreach (var item in list)
                {
                    if (customerMap.TryGetValue(item.Id, out var custId))
                    {
                        item.CustomerName = partyDict.TryGetValue(custId, out var name) ? name : null;
                    }
                }
            }

            return list;
        }

        public async Task<IReadOnlyList<ComplaintForSalesReturnLookupDto>> GetComplaintsForSalesReturnAsync(string term, CancellationToken ct)
        {
            var accessCtx = await _dataAccessFilter.GetContextAsync(ct);
            var allowedPartyIds = new HashSet<int>(accessCtx.AllowedCustomerIds);
            if (accessCtx.PartyId.HasValue)
                allowedPartyIds.Add(accessCtx.PartyId.Value);

            if (accessCtx.IsCustomerRestricted && allowedPartyIds.Count == 0)
                return new List<ComplaintForSalesReturnLookupDto>();

            var partyFilter = accessCtx.IsCustomerRestricted && allowedPartyIds.Count > 0
                ? " AND ch.CustomerId IN @AllowedPartyIds"
                : string.Empty;

            var sql = $@"
                SELECT TOP 20
                    ch.Id,
                    ch.ComplaintNumber,
                    ch.ComplaintDate,
                    ch.CustomerId,
                    rs.Description AS ReturnStatusName
                FROM Sales.ComplaintHeader ch
                INNER JOIN Sales.ComplaintResolution cr
                    ON cr.ComplaintHeaderId = ch.Id AND cr.IsDeleted = 0
                INNER JOIN Sales.MiscMaster rt
                    ON cr.ResolutionTypeId = rt.Id AND rt.IsDeleted = 0
                    AND rt.Code = 'Sales Return'
                -- Resolution workflow must be approved: header status must be a ClosureStatus type
                INNER JOIN Sales.MiscMaster hm
                    ON ch.StatusId = hm.Id AND hm.IsDeleted = 0
                INNER JOIN Sales.MiscTypeMaster mt
                    ON hm.MiscTypeId = mt.Id AND mt.MiscTypeCode = 'ClosureStatus'
                LEFT JOIN Sales.MiscMaster rs
                    ON cr.ReturnStatusId = rs.Id AND rs.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster cs
                    ON cr.ClosureStatusId = cs.Id AND cs.IsDeleted = 0
                WHERE ch.IsDeleted = 0
                  AND (cr.ReturnStatusId IS NULL OR rs.Code <> 'FullyReturned')
                  AND (cs.Code IS NULL OR cs.Code <> 'Closed')
                  AND ch.ComplaintNumber LIKE @Term
                  {partyFilter}
                ORDER BY ch.ComplaintDate DESC;";

            var dp = new DynamicParameters();
            dp.Add("Term", $"%{term}%");
            if (accessCtx.IsCustomerRestricted && allowedPartyIds.Count > 0)
                dp.Add("AllowedPartyIds", allowedPartyIds.ToList());

            var result = await _dbConnection.QueryAsync<ComplaintForSalesReturnLookupDto>(sql, dp);
            var list = result.ToList();

            if (list.Count > 0)
            {
                var customerIds = list.Select(l => l.CustomerId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(customerIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                foreach (var item in list)
                {
                    item.CustomerName = partyDict.TryGetValue(item.CustomerId, out var name) ? name : null;
                }
            }

            return list;
        }

        public async Task<string?> GetAttachmentFilePathAsync(int id)
        {
            const string sql = @"
                SELECT FilePath FROM Sales.ComplaintAttachment
                WHERE Id = @Id AND IsDeleted = 0;";
            return await _dbConnection.ExecuteScalarAsync<string?>(sql, new { Id = id });
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.ComplaintHeader
                WHERE Id = @Id AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            var party = await _partyLookup.GetByIdAsync(customerId);
            return party != null;
        }

        public async Task<bool> InvoiceBelongsToCustomerAsync(int invoiceHeaderId, int customerId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.InvoiceHeader
                WHERE Id = @InvoiceHeaderId AND PartyId = @CustomerId AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { InvoiceHeaderId = invoiceHeaderId, CustomerId = customerId });
            return count > 0;
        }

        public async Task<List<CustomerInvoiceDto>> GetCustomerInvoicesAsync(int customerId)
        {
            // Scope invoices to the current user's unit — prevents cross-unit data leakage
            // when the same customer has invoices raised from multiple units.
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            const string sql = @"
                SELECT
                    ih.Id,
                    ih.InvoiceNo,
                    ih.InvoiceDate,
                    so.SalesOrderTypeId AS InvoiceType,
                    tt.TypeName AS InvoiceTypeName,
                    ih.InvoiceAmount
                FROM Sales.InvoiceHeader ih
                LEFT JOIN Sales.DispatchAdviceHeader da ON ih.DispatchAdviceId = da.Id AND da.IsDeleted = 0
                LEFT JOIN Sales.SalesOrderHeader so ON da.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Finance.TransactionTypeMaster tt ON so.SalesOrderTypeId = tt.Id AND tt.IsDeleted = 0
                WHERE ih.PartyId = @CustomerId AND ih.UnitId = @UnitId AND ih.IsDeleted = 0
                ORDER BY ih.InvoiceDate DESC;";

            var result = await _dbConnection.QueryAsync<CustomerInvoiceDto>(sql, new { CustomerId = customerId, UnitId = unitId });
            return result.ToList();
        }

        public async Task<List<InvoiceLineDetailDto>> GetInvoiceLineDetailsAsync(int invoiceHeaderId)
        {
            const string sql = @"
                SELECT
                    ih.Id AS InvoiceHeaderId,
                    ih.InvoiceNo,
                    ih.InvoiceDate,
                    so.SalesOrderTypeId AS InvoiceType,
                    tt.TypeName AS InvoiceTypeName,
                    id.LotId,
                    id.ItemId,
                    id.NoOfBags,
                    id.NetWeight AS Quantity,
                    id.TotalAmount
                FROM Sales.InvoiceDetail id
                INNER JOIN Sales.InvoiceHeader ih ON id.InvoiceHeaderId = ih.Id AND ih.IsDeleted = 0
                LEFT JOIN Sales.DispatchAdviceHeader da ON ih.DispatchAdviceId = da.Id AND da.IsDeleted = 0
                LEFT JOIN Sales.SalesOrderHeader so ON da.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Finance.TransactionTypeMaster tt ON so.SalesOrderTypeId = tt.Id AND tt.IsDeleted = 0
                WHERE id.InvoiceHeaderId = @InvoiceHeaderId;";

            var result = (await _dbConnection.QueryAsync<InvoiceLineDetailDto>(sql, new { InvoiceHeaderId = invoiceHeaderId })).ToList();

            // Populate cross-module lookups
            if (result.Count > 0)
            {
                var itemIds = result.Select(r => r.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

                var lotIds = result.Where(r => r.LotId.HasValue).Select(r => r.LotId!.Value).Distinct();
                var lots = await _lotLookup.GetByIdsAsync(lotIds);
                var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);

                foreach (var line in result)
                {
                    if (itemDict.TryGetValue(line.ItemId, out var itemInfo))
                    {
                        line.ItemCode = itemInfo.ItemCode;
                        line.ItemName = itemInfo.ItemName;
                    }
                    line.LotCode = line.LotId.HasValue && lotDict.TryGetValue(line.LotId.Value, out var lotCode) ? lotCode : null;
                }
            }

            return result;
        }

        public async Task<(List<InvoiceSearchDto>, int)> SearchInvoicesAsync(int partyId, string? searchTerm, bool lastOneYear, int pageNumber, int pageSize)
        {
            // Scope invoices to the current user's unit — without this, customers with invoices
            // across multiple units leak rows from other units into the Pull-from-Invoice list.
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            var dateFilter = lastOneYear
                ? " AND ih.InvoiceDate >= DATEADD(YEAR, -1, GETDATE())"
                : string.Empty;

            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : @" AND (ih.InvoiceNo LIKE @SearchTerm
                       OR tt.TypeName LIKE @SearchTerm
                       OR tt.ShortName LIKE @SearchTerm)";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.InvoiceDetail id
                INNER JOIN Sales.InvoiceHeader ih ON id.InvoiceHeaderId = ih.Id AND ih.IsDeleted = 0
                LEFT JOIN Sales.DispatchAdviceHeader da ON ih.DispatchAdviceId = da.Id AND da.IsDeleted = 0
                LEFT JOIN Sales.SalesOrderHeader so ON da.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Finance.TransactionTypeMaster tt ON so.SalesOrderTypeId = tt.Id AND tt.IsDeleted = 0
                WHERE ih.PartyId = @PartyId AND ih.UnitId = @UnitId {dateFilter} {searchFilter};";

            var dataSql = $@"
                SELECT
                    ROW_NUMBER() OVER (ORDER BY ih.InvoiceDate DESC, ih.Id, id.ItemSno) AS Sno,
                    id.Id AS InvoiceDetailId,
                    ih.Id AS InvoiceHeaderId,
                    ih.InvoiceDate,
                    so.SalesOrderTypeId AS InvoiceTypeId,
                    tt.TypeName AS InvoiceTypeName,
                    ih.InvoiceNo,
                    tt.ShortName AS ShortCode,
                    id.ItemId,
                    id.LotId,
                    ih.UnitId,
                    id.NoOfBags AS Packs,
                    id.BagWeight AS NetWeight,
                    id.TotalAmount AS InvAmount,
                    id.UOMId
                FROM Sales.InvoiceDetail id
                INNER JOIN Sales.InvoiceHeader ih ON id.InvoiceHeaderId = ih.Id AND ih.IsDeleted = 0
                LEFT JOIN Sales.DispatchAdviceHeader da ON ih.DispatchAdviceId = da.Id AND da.IsDeleted = 0
                LEFT JOIN Sales.SalesOrderHeader so ON da.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Finance.TransactionTypeMaster tt ON so.SalesOrderTypeId = tt.Id AND tt.IsDeleted = 0
                WHERE ih.PartyId = @PartyId AND ih.UnitId = @UnitId {dateFilter} {searchFilter}
                ORDER BY ih.InvoiceDate DESC, ih.Id, id.ItemSno
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                PartyId = partyId,
                UnitId = unitId,
                SearchTerm = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(countSql + dataSql, parameters);
            var totalCount = await multi.ReadFirstAsync<int>();
            var data = (await multi.ReadAsync<InvoiceSearchDto>()).ToList();

            if (data.Count > 0)
            {
                // Populate ProductCode/ProductName via cross-module item lookup
                var itemIds = data.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

                // Populate LotNum via cross-module lot lookup
                var lotIds = data.Where(d => d.LotId.HasValue).Select(d => d.LotId!.Value).Distinct();
                var lots = await _lotLookup.GetByIdsAsync(lotIds);
                var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);

                // Populate Division from Unit (Division = Unit in this system)
                var unitIds = data.Select(d => d.UnitId).Where(u => u > 0).Distinct();
                var units = await _unitLookup.GetByIdsAsync(unitIds);
                var unitDict = units.ToDictionary(u => u.UnitId, u => (u.UnitId, u.UnitName));

                // Populate UOM names
                var uomIds = data.Where(d => d.UOMId.HasValue).Select(d => d.UOMId!.Value).Distinct();
                var uoms = await _uomLookup.GetByIdsAsync(uomIds);
                var uomDict = uoms.ToDictionary(u => u.Id, u => u.UOMName);

                foreach (var row in data)
                {
                    if (itemDict.TryGetValue(row.ItemId, out var itemInfo))
                    {
                        row.ProductCode = itemInfo.ItemCode;
                        row.ProductName = itemInfo.ItemName;
                    }

                    row.LotNum = row.LotId.HasValue && lotDict.TryGetValue(row.LotId.Value, out var lotCode) ? lotCode : null;

                    if (unitDict.TryGetValue(row.UnitId, out var unitInfo))
                    {
                        row.DivisionId = unitInfo.UnitId;
                        row.DivisionName = unitInfo.UnitName;
                    }

                    if (row.UOMId.HasValue && uomDict.TryGetValue(row.UOMId.Value, out var uomName))
                    {
                        row.UOMName = uomName;
                    }
                }
            }

            return (data, totalCount);
        }

        public async Task<(List<PendingQCReviewListDto>, int)> GetPendingQCReviewAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : " AND (ch.ComplaintNumber LIKE @SearchTerm OR ch.Remarks LIKE @SearchTerm)";

            var sql = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.ComplaintQCReview qr
                INNER JOIN Sales.ComplaintHeader ch ON ch.Id = qr.ComplaintHeaderId AND ch.IsDeleted = 0
                WHERE qr.IsDeleted = 0
                  AND qr.ReviewedBy IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM Sales.MiscMaster hm
                      INNER JOIN Sales.MiscTypeMaster mt ON hm.MiscTypeId = mt.Id
                      WHERE hm.Id = ch.StatusId AND mt.MiscTypeCode = 'QCComplaintStatus' AND hm.IsDeleted = 0
                  ) {searchFilter};

                SELECT
                    qr.Id,
                    qr.ComplaintHeaderId,
                    ch.ComplaintNumber,
                    ch.ComplaintDate,
                    ch.CustomerId,
                    pv.Description AS PhysicalVerificationName,
                    qr.LabVerificationRequired,
                    sv.Description AS SeverityName,
                    qr.Comments,
                    qr.CreatedByName,
                    qr.CreatedDate
                FROM Sales.ComplaintQCReview qr
                INNER JOIN Sales.ComplaintHeader ch ON ch.Id = qr.ComplaintHeaderId AND ch.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster pv ON qr.PhysicalVerificationId = pv.Id AND pv.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster sv ON qr.SeverityId = sv.Id AND sv.IsDeleted = 0
                WHERE qr.IsDeleted = 0
                  AND qr.ReviewedBy IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM Sales.MiscMaster hm
                      INNER JOIN Sales.MiscTypeMaster mt ON hm.MiscTypeId = mt.Id
                      WHERE hm.Id = ch.StatusId AND mt.MiscTypeCode = 'QCComplaintStatus' AND hm.IsDeleted = 0
                  ) {searchFilter}
                ORDER BY qr.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount;";

            var parameters = new
            {
                SearchTerm = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var data = (await multi.ReadAsync<PendingQCReviewListDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (data.Count > 0)
            {
                var partyIds = data.Select(d => d.CustomerId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                foreach (var item in data)
                    item.CustomerName = partyDict.TryGetValue(item.CustomerId, out var name) ? name : null;
            }

            return (data, totalCount);
        }

        public async Task<(List<PendingResolutionListDto>, int)> GetPendingResolutionAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : " AND (ch.ComplaintNumber LIKE @SearchTerm OR ch.Remarks LIKE @SearchTerm)";

            var sql = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.ComplaintResolution cr
                INNER JOIN Sales.ComplaintHeader ch ON ch.Id = cr.ComplaintHeaderId AND ch.IsDeleted = 0
                INNER JOIN Sales.MiscMaster cs ON cr.ClosureStatusId = cs.Id AND cs.IsDeleted = 0
                WHERE cr.IsDeleted = 0 AND cs.Description <> @ClosedStatus {searchFilter};

                SELECT
                    cr.Id,
                    cr.ComplaintHeaderId,
                    ch.ComplaintNumber,
                    ch.ComplaintDate,
                    ch.CustomerId,
                    rt.Description AS ResolutionTypeName,
                    cr.ResolutionSummary,
                    cs.Description AS ClosureStatusName,
                    cr.CreatedByName,
                    cr.CreatedDate
                FROM Sales.ComplaintResolution cr
                INNER JOIN Sales.ComplaintHeader ch ON ch.Id = cr.ComplaintHeaderId AND ch.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster rt ON cr.ResolutionTypeId = rt.Id AND rt.IsDeleted = 0
                INNER JOIN Sales.MiscMaster cs ON cr.ClosureStatusId = cs.Id AND cs.IsDeleted = 0
                WHERE cr.IsDeleted = 0 AND cs.Description <> @ClosedStatus {searchFilter}
                ORDER BY cr.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount;";

            var parameters = new
            {
                SearchTerm = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize,
                ClosedStatus = MiscEnumEntity.ClosureStatusClosed
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var data = (await multi.ReadAsync<PendingResolutionListDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (data.Count > 0)
            {
                var partyIds = data.Select(d => d.CustomerId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                foreach (var item in data)
                    item.CustomerName = partyDict.TryGetValue(item.CustomerId, out var name) ? name : null;
            }

            return (data, totalCount);
        }

        public async Task<(List<PendingComplaintListDto>, int)> GetPendingComplaintsAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : " AND (h.ComplaintNumber LIKE @SearchTerm OR ms.Description LIKE @SearchTerm OR h.Remarks LIKE @SearchTerm)";

            var sql = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.ComplaintHeader h
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND ms.Code = 'Pending' {searchFilter};

                SELECT
                    h.Id,
                    h.ComplaintNumber,
                    h.ComplaintDate,
                    h.CustomerId,
                    h.StatusId,
                    ms.Description AS StatusName,
                    h.Remarks,
                    h.CreatedByName,
                    h.CreatedDate
                FROM Sales.ComplaintHeader h
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND ms.Code = 'Pending' {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount;";

            var parameters = new
            {
                SearchTerm = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var data = (await multi.ReadAsync<PendingComplaintListDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (data.Count > 0)
            {
                var partyIds = data.Select(d => d.CustomerId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                foreach (var item in data)
                    item.CustomerName = partyDict.TryGetValue(item.CustomerId, out var name) ? name : null;
            }

            return (data, totalCount);
        }

        public async Task<bool> IsReadyForResolutionAsync(int complaintHeaderId)
        {
            const string sql = @"
                SELECT CASE WHEN
                    -- QC Review form was submitted (reviewer filled in the review)
                    EXISTS (
                        SELECT 1 FROM Sales.ComplaintQCReview qr
                        WHERE qr.ComplaintHeaderId = @Id
                          AND qr.ReviewedBy IS NOT NULL
                          AND qr.ComplaintStatusId IS NOT NULL
                          AND qr.IsDeleted = 0
                    )
                    -- QC Workflow was approved: complaint header status is a QCComplaintStatus type
                    AND EXISTS (
                        SELECT 1 FROM Sales.ComplaintHeader ch
                        INNER JOIN Sales.MiscMaster mm ON ch.StatusId = mm.Id
                        INNER JOIN Sales.MiscTypeMaster mmt ON mm.MiscTypeId = mmt.Id
                        WHERE ch.Id = @Id
                          AND mmt.MiscTypeCode = 'QCComplaintStatus'
                          AND mm.IsDeleted = 0
                          AND ch.IsDeleted = 0
                    )
                    -- All mandatory assignments have feedback submitted
                    AND NOT EXISTS (
                        SELECT 1 FROM Sales.ComplaintQCReviewAssignment a
                        INNER JOIN Sales.ComplaintQCReview qr ON a.ComplaintQCReviewId = qr.Id
                        INNER JOIN Sales.MiscMaster mm ON a.AssignmentStatusId = mm.Id
                        WHERE qr.ComplaintHeaderId = @Id
                          AND a.IsMandatory = 1
                          AND mm.Description != 'Submitted'
                          AND a.IsDeleted = 0
                          AND qr.IsDeleted = 0
                    )
                THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = complaintHeaderId });
        }

        public async Task<bool> IsComplaintFinalizedAsync(int id)
        {
            // True when the complaint's current StatusName is "QC Accepted" or "Closed"
            // (matched case-insensitively, trimmed) — these are the terminal states from
            // QC and Resolution workflows respectively. Used by UpdateComplaintCommandValidator.
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM   Sales.ComplaintHeader ch
                    INNER  JOIN Sales.MiscMaster mm ON mm.Id = ch.StatusId
                    WHERE  ch.Id = @Id
                      AND  ch.IsDeleted = 0
                      AND  mm.IsDeleted = 0
                      AND  UPPER(LTRIM(RTRIM(mm.Description))) IN ('QC ACCEPTED', 'CLOSED')
                ) THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }
    }
}
