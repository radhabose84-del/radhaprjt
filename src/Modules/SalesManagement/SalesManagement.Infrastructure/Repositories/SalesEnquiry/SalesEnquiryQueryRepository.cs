using System.Data;
using Dapper;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Inventory;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Dto;

namespace SalesManagement.Infrastructure.Repositories.SalesEnquiry
{
    public class SalesEnquiryQueryRepository : ISalesEnquiryQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IPaymentTermLookup _paymentTermLookup;
        private readonly IItemLookup _itemLookup;

        public SalesEnquiryQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IPaymentTermLookup paymentTermLookup,
            IItemLookup itemLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _paymentTermLookup = paymentTermLookup;
            _itemLookup = itemLookup;
        }

        public async Task<(List<SalesEnquiryHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (h.ContactPerson LIKE @Search OR h.Remarks LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesEnquiryHeader h
                WHERE h.IsDeleted = 0 {searchFilter};

                SELECT h.Id, h.PartyId, h.EnquiryDate, h.ContactPerson,
                    h.ExpectedDeliveryDate, h.PaymentTermId, h.SalesLeadId,
                    SL.ProspectCompanyName AS SalesLeadProspectName,
                    h.Remarks, h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Sales.SalesEnquiryHeader h
                LEFT JOIN Sales.SalesLead SL ON SL.Id = h.SalesLeadId AND SL.IsDeleted = 0
                WHERE h.IsDeleted = 0 {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<SalesEnquiryHeaderDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            // Populate cross-module lookup names
            if (list.Count > 0)
            {
                var partyIds = list.Select(x => x.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                var ptDict = paymentTerms.ToDictionary(p => p.Id, p => p.Description);

                foreach (var item in list)
                {
                    item.PartyName = partyDict.TryGetValue(item.PartyId, out var pName) ? pName : null;
                    if (item.PaymentTermId.HasValue)
                        item.PaymentTermDescription = ptDict.TryGetValue(item.PaymentTermId.Value, out var ptName) ? ptName : null;
                }
            }

            return (list, totalCount);
        }

        public async Task<SalesEnquiryHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT h.Id, h.PartyId, h.EnquiryDate, h.ContactPerson,
                    h.ExpectedDeliveryDate, h.PaymentTermId, h.SalesLeadId,
                    SL.ProspectCompanyName AS SalesLeadProspectName,
                    h.Remarks, h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName
                FROM Sales.SalesEnquiryHeader h
                LEFT JOIN Sales.SalesLead SL ON SL.Id = h.SalesLeadId AND SL.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<SalesEnquiryHeaderDto>(headerSql, new { Id = id });

            if (header == null)
                return null;

            const string detailSql = @"
                SELECT d.Id, d.SalesEnquiryHeaderId, d.ItemId,
                    d.Quantity, d.ExmillRate, d.TargetPrice, d.Discount
                FROM Sales.SalesEnquiryDetail d
                WHERE d.SalesEnquiryHeaderId = @HeaderId";

            var details = (await _dbConnection.QueryAsync<SalesEnquiryDetailDto>(detailSql, new { HeaderId = id })).ToList();

            // Populate party name
            var parties = await _partyLookup.GetByIdsAsync(new[] { header.PartyId });
            var party = parties.FirstOrDefault();
            header.PartyName = party?.PartyName;

            // Populate payment term description
            if (header.PaymentTermId.HasValue)
            {
                var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                var pt = paymentTerms.FirstOrDefault(p => p.Id == header.PaymentTermId.Value);
                header.PaymentTermDescription = pt?.Description;
            }

            // Populate item names on details
            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

                foreach (var detail in details)
                {
                    if (itemDict.TryGetValue(detail.ItemId, out var itemInfo))
                    {
                        detail.ItemCode = itemInfo.ItemCode;
                        detail.ItemName = itemInfo.ItemName;
                    }
                }
            }

            header.SalesEnquiryDetails = details;
            return header;
        }

        public async Task<IReadOnlyList<SalesEnquiryLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            var searchFilter = string.IsNullOrWhiteSpace(term)
                ? ""
                : "AND (h.ContactPerson LIKE @Term OR h.Remarks LIKE @Term)";

            var sql = $@"
                SELECT h.Id, h.PartyId, h.EnquiryDate,
                    (SELECT COUNT(*) FROM Sales.SalesEnquiryDetail d
                     WHERE d.SalesEnquiryHeaderId = h.Id) AS TotalItems
                FROM Sales.SalesEnquiryHeader h
                WHERE h.IsActive = 1 AND h.IsDeleted = 0
                {searchFilter}
                ORDER BY h.EnquiryDate DESC";

            var list = (await _dbConnection.QueryAsync<SalesEnquiryLookupDto>(
                sql, new { Term = $"%{term}%" })).ToList();

            if (list.Count > 0)
            {
                var partyIds = list.Select(x => x.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                foreach (var item in list)
                    item.PartyName = partyDict.TryGetValue(item.PartyId, out var name) ? name : null;
            }

            return list;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesEnquiryHeader
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> PartyExistsAsync(int partyId)
        {
            var parties = await _partyLookup.GetByIdsAsync(new[] { partyId });
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

        public async Task<bool> SalesLeadExistsAsync(int salesLeadId)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.SalesLead WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesLeadId });
            return count > 0;
        }
    }
}
