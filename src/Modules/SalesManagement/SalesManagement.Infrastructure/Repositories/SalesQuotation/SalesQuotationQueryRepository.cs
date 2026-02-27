using System.Data;
using Dapper;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Inventory;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Dto;

namespace SalesManagement.Infrastructure.Repositories.SalesQuotation
{
    public class SalesQuotationQueryRepository : ISalesQuotationQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IPaymentTermLookup _paymentTermLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IHSNLookup _hsnLookup;

        public SalesQuotationQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IPaymentTermLookup paymentTermLookup,
            IItemLookup itemLookup,
            IHSNLookup hsnLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _paymentTermLookup = paymentTermLookup;
            _itemLookup = itemLookup;
            _hsnLookup = hsnLookup;
        }

        public async Task<(List<SalesQuotationHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (h.Remarks LIKE @Search OR sc.ContactName LIKE @Search OR mm.Description LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesQuotationHeader h
                LEFT JOIN Sales.SalesContact sc ON h.ContactPersonId = sc.Id AND sc.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON h.DeliveryTermId = mm.Id AND mm.IsDeleted = 0
                WHERE h.IsDeleted = 0 {searchFilter};

                SELECT h.Id, h.CustomerId, h.QuotationDate,
                    h.SalesEnquiryId, h.ContactPersonId,
                    sc.ContactName AS ContactPersonName,
                    h.ValidityDate, h.PaymentTermId, h.Remarks,
                    h.DeliveryTermId,
                    mm.Description AS DeliveryTermDescription,
                    h.FreightCharges, h.OtherCharges,
                    h.TotalBasicAmount, h.TotalDiscount,
                    h.NetTaxableAmount, h.TotalTax, h.GrandTotal,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Sales.SalesQuotationHeader h
                LEFT JOIN Sales.SalesContact sc ON h.ContactPersonId = sc.Id AND sc.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON h.DeliveryTermId = mm.Id AND mm.IsDeleted = 0
                WHERE h.IsDeleted = 0 {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<SalesQuotationHeaderDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                // Fetch details for the paginated headers from SalesQuotationDetail table
                var headerIds = list.Select(x => x.Id).ToArray();
                const string detailSql = @"
                    SELECT d.Id, d.SalesQuotationHeaderId, d.ItemId,
                        d.Quantity, d.ExMillRate, d.Discount,
                        d.NetRate, d.TotalAmount,
                        d.HSNId, d.TaxPercentage, d.TaxAmount
                    FROM Sales.SalesQuotationDetail d
                    WHERE d.SalesQuotationHeaderId IN @HeaderIds";

                var allDetails = (await _dbConnection.QueryAsync<SalesQuotationDetailDto>(detailSql, new { HeaderIds = headerIds })).ToList();

                // Populate cross-module lookup names
                var customerIds = list.Select(x => x.CustomerId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(customerIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                var ptDict = paymentTerms.ToDictionary(p => p.Id, p => p.Description);

                // Populate item and HSN names on details
                if (allDetails.Count > 0)
                {
                    var itemIds = allDetails.Select(d => d.ItemId).Distinct();
                    var items = await _itemLookup.GetByIdsAsync(itemIds);
                    var itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

                    var hsnIds = allDetails.Select(d => d.HSNId).Distinct();
                    var hsnList = await _hsnLookup.GetByIdsAsync(hsnIds);
                    var hsnDict = hsnList.ToDictionary(h => h.Id, h => (h.HSNCode, h.Description));

                    foreach (var detail in allDetails)
                    {
                        if (itemDict.TryGetValue(detail.ItemId, out var itemInfo))
                        {
                            detail.ItemCode = itemInfo.ItemCode;
                            detail.ItemName = itemInfo.ItemName;
                        }

                        if (hsnDict.TryGetValue(detail.HSNId, out var hsnInfo))
                        {
                            detail.HSNCode = hsnInfo.HSNCode;
                            detail.HSNDescription = hsnInfo.Description;
                        }
                    }
                }

                // Group details by header and attach
                var detailsByHeader = allDetails.GroupBy(d => d.SalesQuotationHeaderId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var item in list)
                {
                    item.CustomerName = partyDict.TryGetValue(item.CustomerId, out var pName) ? pName : null;
                    item.PaymentTermDescription = ptDict.TryGetValue(item.PaymentTermId, out var ptName) ? ptName : null;
                    item.SalesQuotationDetails = detailsByHeader.TryGetValue(item.Id, out var details) ? details : [];
                }
            }

            return (list, totalCount);
        }

        public async Task<SalesQuotationHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT h.Id, h.CustomerId, h.QuotationDate,
                    h.SalesEnquiryId, h.ContactPersonId,
                    sc.ContactName AS ContactPersonName,
                    h.ValidityDate, h.PaymentTermId, h.Remarks,
                    h.DeliveryTermId,
                    mm.Description AS DeliveryTermDescription,
                    h.FreightCharges, h.OtherCharges,
                    h.TotalBasicAmount, h.TotalDiscount,
                    h.NetTaxableAmount, h.TotalTax, h.GrandTotal,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Sales.SalesQuotationHeader h
                LEFT JOIN Sales.SalesContact sc ON h.ContactPersonId = sc.Id AND sc.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON h.DeliveryTermId = mm.Id AND mm.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<SalesQuotationHeaderDto>(headerSql, new { Id = id });

            if (header == null)
                return null;

            const string detailSql = @"
                SELECT d.Id, d.SalesQuotationHeaderId, d.ItemId,
                    d.Quantity, d.ExMillRate, d.Discount,
                    d.NetRate, d.TotalAmount,
                    d.HSNId, d.TaxPercentage, d.TaxAmount
                FROM Sales.SalesQuotationDetail d
                WHERE d.SalesQuotationHeaderId = @HeaderId";

            var details = (await _dbConnection.QueryAsync<SalesQuotationDetailDto>(detailSql, new { HeaderId = id })).ToList();

            // Populate customer name
            var parties = await _partyLookup.GetByIdsAsync(new[] { header.CustomerId });
            var party = parties.FirstOrDefault();
            header.CustomerName = party?.PartyName;

            // Populate payment term description
            var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
            var pt = paymentTerms.FirstOrDefault(p => p.Id == header.PaymentTermId);
            header.PaymentTermDescription = pt?.Description;

            // Populate item and HSN names on details
            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

                var hsnIds = details.Select(d => d.HSNId).Distinct();
                var hsnList = await _hsnLookup.GetByIdsAsync(hsnIds);
                var hsnDict = hsnList.ToDictionary(h => h.Id, h => (h.HSNCode, h.Description));

                foreach (var detail in details)
                {
                    if (itemDict.TryGetValue(detail.ItemId, out var itemInfo))
                    {
                        detail.ItemCode = itemInfo.ItemCode;
                        detail.ItemName = itemInfo.ItemName;
                    }

                    if (hsnDict.TryGetValue(detail.HSNId, out var hsnInfo))
                    {
                        detail.HSNCode = hsnInfo.HSNCode;
                        detail.HSNDescription = hsnInfo.Description;
                    }
                }
            }

            header.SalesQuotationDetails = details;
            return header;
        }

        public async Task<IReadOnlyList<SalesQuotationLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT h.Id, h.QuotationDate, h.GrandTotal,
                    (SELECT COUNT(*) FROM Sales.SalesQuotationDetail d WHERE d.SalesQuotationHeaderId = h.Id) AS TotalItems
                FROM Sales.SalesQuotationHeader h
                WHERE h.IsActive = 1 AND h.IsDeleted = 0
                AND (CAST(h.Id AS VARCHAR) LIKE @Term OR h.Remarks LIKE @Term)
                ORDER BY h.Id DESC";

            var list = (await _dbConnection.QueryAsync<SalesQuotationLookupDto>(sql, new { Term = $"%{term}%" })).ToList();

            // Populate customer names
            if (list.Count > 0)
            {
                // Get header customer IDs for lookup
                const string customerSql = @"
                    SELECT h.Id, h.CustomerId
                    FROM Sales.SalesQuotationHeader h
                    WHERE h.IsActive = 1 AND h.IsDeleted = 0
                    AND h.Id IN @Ids";

                var headerIds = list.Select(x => x.Id).ToArray();
                var headers = await _dbConnection.QueryAsync<(int Id, int CustomerId)>(customerSql, new { Ids = headerIds });
                var headerCustomerDict = headers.ToDictionary(h => h.Id, h => h.CustomerId);

                var customerIds = headerCustomerDict.Values.Distinct();
                var parties = await _partyLookup.GetByIdsAsync(customerIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                foreach (var item in list)
                {
                    if (headerCustomerDict.TryGetValue(item.Id, out var customerId))
                    {
                        item.CustomerName = partyDict.TryGetValue(customerId, out var name) ? name : null;
                    }
                }
            }

            return list;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesQuotationHeader
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            var parties = await _partyLookup.GetByIdsAsync(new[] { customerId });
            return parties.Any();
        }

        public async Task<bool> PaymentTermExistsAsync(int paymentTermId)
        {
            var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
            return paymentTerms.Any(p => p.Id == paymentTermId);
        }

        public async Task<bool> ItemExistsAsync(int itemId)
        {
            var items = await _itemLookup.GetByIdsAsync(new[] { itemId });
            return items.Any();
        }

        public async Task<bool> HSNExistsAsync(int hsnId)
        {
            var hsnList = await _hsnLookup.GetByIdsAsync(new[] { hsnId });
            return hsnList.Any();
        }

        public async Task<bool> DeliveryTermExistsAsync(int deliveryTermId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MiscMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = deliveryTermId });
            return count > 0;
        }

        public async Task<bool> ContactPersonExistsAsync(int contactPersonId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesContact
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = contactPersonId });
            return count > 0;
        }

        public async Task<bool> SalesEnquiryExistsAsync(int salesEnquiryId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesEnquiryHeader
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesEnquiryId });
            return count > 0;
        }
    }
}
