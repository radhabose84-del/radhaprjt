using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Infrastructure.Repositories.Complaint
{
    public class ComplaintQueryRepository : IComplaintQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IDivisionLookup _divisionLookup;

        public ComplaintQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IItemLookup itemLookup,
            IDivisionLookup divisionLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
            _divisionLookup = divisionLookup;
        }

        public async Task<(List<ComplaintHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : @" AND (h.ComplaintNumber LIKE @SearchTerm
                       OR ms.Description LIKE @SearchTerm
                       OR h.Remarks LIKE @SearchTerm)";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.ComplaintHeader h
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.IsDeleted = 0 {searchFilter};";

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
                WHERE h.IsDeleted = 0 {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                SearchTerm = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(countSql + dataSql, parameters);
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

            // Fetch details with same-module JOINs
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
                    lm.LotCode,
                    d.ItemId,
                    d.NumberOfPacks,
                    d.NetWeight,
                    d.InvoiceAmount,
                    d.DivisionId
                FROM Sales.ComplaintDetail d
                LEFT JOIN Sales.InvoiceHeader ih ON d.InvoiceHeaderId = ih.Id AND ih.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mt ON d.InvoiceTypeId = mt.Id AND mt.IsDeleted = 0
                LEFT JOIN Production.LotMaster lm ON d.LotId = lm.Id AND lm.IsDeleted = 0
                WHERE d.ComplaintHeaderId = @HeaderId AND d.IsDeleted = 0;";

            var details = (await _dbConnection.QueryAsync<ComplaintDetailDto>(detailSql, new { HeaderId = id })).ToList();

            // Populate cross-module lookups for details
            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

                var divisionIds = details.Where(d => d.DivisionId.HasValue).Select(d => d.DivisionId!.Value).Distinct();
                var divisions = await _divisionLookup.GetByIdsAsync(divisionIds);
                var divisionDict = divisions.ToDictionary(d => d.Id, d => d.Name);

                foreach (var detail in details)
                {
                    if (itemDict.TryGetValue(detail.ItemId, out var itemInfo))
                    {
                        detail.ItemCode = itemInfo.ItemCode;
                        detail.ItemName = itemInfo.ItemName;
                    }
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
            return header;
        }

        public async Task<IReadOnlyList<ComplaintLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20
                    h.Id,
                    h.ComplaintNumber,
                    h.ComplaintDate
                FROM Sales.ComplaintHeader h
                WHERE h.IsDeleted = 0 AND h.IsActive = 1
                  AND h.ComplaintNumber LIKE @Term
                ORDER BY h.ComplaintNumber ASC;";

            var result = await _dbConnection.QueryAsync<ComplaintLookupDto>(sql, new { Term = $"%{term}%" });
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
            const string sql = @"
                SELECT
                    ih.Id,
                    ih.InvoiceNo,
                    ih.InvoiceDate,
                    ih.InvoiceType,
                    mm.Description AS InvoiceTypeName,
                    ih.InvoiceAmount
                FROM Sales.InvoiceHeader ih
                LEFT JOIN Sales.MiscMaster mm ON ih.InvoiceType = mm.Id AND mm.IsDeleted = 0
                WHERE ih.PartyId = @CustomerId AND ih.IsDeleted = 0
                ORDER BY ih.InvoiceDate DESC;";

            var result = await _dbConnection.QueryAsync<CustomerInvoiceDto>(sql, new { CustomerId = customerId });
            return result.ToList();
        }

        public async Task<List<InvoiceLineDetailDto>> GetInvoiceLineDetailsAsync(int invoiceHeaderId)
        {
            const string sql = @"
                SELECT
                    ih.Id AS InvoiceHeaderId,
                    ih.InvoiceNo,
                    ih.InvoiceDate,
                    ih.InvoiceType,
                    mm.Description AS InvoiceTypeName,
                    id.LotId,
                    lm.LotCode,
                    id.ItemId,
                    id.NoOfBags,
                    id.Quantity,
                    id.TotalAmount
                FROM Sales.InvoiceDetail id
                INNER JOIN Sales.InvoiceHeader ih ON id.InvoiceHeaderId = ih.Id AND ih.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON ih.InvoiceType = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Production.LotMaster lm ON id.LotId = lm.Id AND lm.IsDeleted = 0
                WHERE id.InvoiceHeaderId = @InvoiceHeaderId;";

            var result = (await _dbConnection.QueryAsync<InvoiceLineDetailDto>(sql, new { InvoiceHeaderId = invoiceHeaderId })).ToList();

            // Populate ItemCode/ItemName via cross-module lookup
            if (result.Count > 0)
            {
                var itemIds = result.Select(r => r.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

                foreach (var line in result)
                {
                    if (itemDict.TryGetValue(line.ItemId, out var itemInfo))
                    {
                        line.ItemCode = itemInfo.ItemCode;
                        line.ItemName = itemInfo.ItemName;
                    }
                }
            }

            return result;
        }
    }
}
