using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Dto;

namespace SalesManagement.Infrastructure.Repositories.SalesLead
{
    public class SalesLeadQueryRepository : ISalesLeadQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly ICityLookup _cityLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IMarketingOfficerAccessFilter _accessFilter;
        private readonly IIPAddressService _ipAddressService;

        public SalesLeadQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            ICityLookup cityLookup,
            IItemLookup itemLookup,
            IMarketingOfficerAccessFilter accessFilter,
            IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _cityLookup = cityLookup;
            _itemLookup = itemLookup;
            _accessFilter = accessFilter;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<SalesLeadDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var whereClause = "sl.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (sl.ContactName LIKE @Search OR sl.MobileNumber LIKE @Search OR sl.ProspectCompanyName LIKE @Search)";

            // Marketing Officer access scoping: own records OR records assigned to officer OR records with mapped customer
            var parameters = new DynamicParameters();
            parameters.Add("Search", $"%{searchTerm}%");
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            if (_accessFilter.IsMarketingOfficer())
            {
                var empId = _accessFilter.GetCurrentMarketingOfficerId();
                var userId = _ipAddressService.GetUserId();
                var customerIds = await _accessFilter.GetAccessibleCustomerIdsAsync();
                var safeIds = customerIds.Count > 0 ? customerIds.ToArray() : new[] { -1 };

                whereClause += " AND (sl.CreatedBy = @UserId OR sl.MarketingOfficerId = @EmpId OR sl.PartyId IN @CustomerIds) ";
                parameters.Add("UserId", userId);
                parameters.Add("EmpId", empId);
                parameters.Add("CustomerIds", safeIds);
            }

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesLead sl
                WHERE {whereClause};

                SELECT sl.Id, sl.PartyId, sl.ProspectCompanyName, sl.CityId,
                    sl.ContactName, sl.MobileNumber, sl.EmailId, sl.ContactId,
                    sc.ContactName AS ExistingContactName,
                    sl.ItemId, sl.RequirementQty, sl.ExpectedDate, sl.Remarks,
                    sl.LeadSourceId, mm.Description AS LeadSourceName,
                    sl.MarketingOfficerId,
                    mo.EmployeeName AS MarketingOfficerName,
                    sl.InteractionDate,
                    sl.IsActive, sl.IsDeleted,
                    sl.CreatedBy, sl.CreatedDate, sl.CreatedByName, sl.CreatedIP,
                    sl.ModifiedBy, sl.ModifiedDate, sl.ModifiedByName, sl.ModifiedIP
                FROM Sales.SalesLead sl
                LEFT JOIN Sales.SalesContact sc ON sl.ContactId = sc.Id AND sc.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON sl.LeadSourceId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.MarketingOfficer mo ON sl.MarketingOfficerId = mo.Id AND mo.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY sl.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<SalesLeadDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                // Cross-module: Party names
                var partyIds = list
                    .Where(x => x.PartyId.HasValue)
                    .Select(x => x.PartyId!.Value)
                    .Distinct();
                if (partyIds.Any())
                {
                    var parties = await _partyLookup.GetByIdsAsync(partyIds);
                    var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                    foreach (var item in list.Where(x => x.PartyId.HasValue))
                        item.PartyName = partyDict.TryGetValue(item.PartyId!.Value, out var name) ? name : null;
                }

                // Cross-module: City names
                var cityIds = list
                    .Where(x => x.CityId.HasValue)
                    .Select(x => x.CityId!.Value)
                    .Distinct();
                if (cityIds.Any())
                {
                    var cities = await _cityLookup.GetByIdsAsync(cityIds);
                    var cityDict = cities.ToDictionary(c => c.CityId, c => c.CityName);
                    foreach (var item in list.Where(x => x.CityId.HasValue))
                        item.CityName = cityDict.TryGetValue(item.CityId!.Value, out var name) ? name : null;
                }

                // Cross-module: Item names
                var itemIds = list
                    .Where(x => x.ItemId.HasValue)
                    .Select(x => x.ItemId!.Value)
                    .Distinct();
                if (itemIds.Any())
                {
                    var items = await _itemLookup.GetByIdsAsync(itemIds);
                    var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);
                    foreach (var item in list.Where(x => x.ItemId.HasValue))
                        item.ItemName = itemDict.TryGetValue(item.ItemId!.Value, out var name) ? name : null;
                }

                // MarketingOfficerName is populated via SQL JOIN — no cross-module lookup needed
            }

            return (list, totalCount);
        }

        public async Task<SalesLeadDto?> GetByIdAsync(int id)
        {
            // Marketing Officer access scoping
            var moFilter = "";
            var parameters = new DynamicParameters();
            parameters.Add("Id", id);

            if (_accessFilter.IsMarketingOfficer())
            {
                var empId = _accessFilter.GetCurrentMarketingOfficerId();
                var userId = _ipAddressService.GetUserId();
                var customerIds = await _accessFilter.GetAccessibleCustomerIdsAsync();
                var safeIds = customerIds.Count > 0 ? customerIds.ToArray() : new[] { -1 };

                moFilter = " AND (sl.CreatedBy = @UserId OR sl.MarketingOfficerId = @EmpId OR sl.PartyId IN @CustomerIds) ";
                parameters.Add("UserId", userId);
                parameters.Add("EmpId", empId);
                parameters.Add("CustomerIds", safeIds);
            }

            var sql = $@"
                SELECT sl.Id, sl.PartyId, sl.ProspectCompanyName, sl.CityId,
                    sl.ContactName, sl.MobileNumber, sl.EmailId, sl.ContactId,
                    sc.ContactName AS ExistingContactName,
                    sl.ItemId, sl.RequirementQty, sl.ExpectedDate, sl.Remarks,
                    sl.LeadSourceId, mm.Description AS LeadSourceName,
                    sl.MarketingOfficerId,
                    mo.EmployeeName AS MarketingOfficerName,
                    sl.InteractionDate,
                    sl.IsActive, sl.IsDeleted,
                    sl.CreatedBy, sl.CreatedDate, sl.CreatedByName, sl.CreatedIP,
                    sl.ModifiedBy, sl.ModifiedDate, sl.ModifiedByName, sl.ModifiedIP
                FROM Sales.SalesLead sl
                LEFT JOIN Sales.SalesContact sc ON sl.ContactId = sc.Id AND sc.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON sl.LeadSourceId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.MarketingOfficer mo ON sl.MarketingOfficerId = mo.Id AND mo.IsDeleted = 0
                WHERE sl.Id = @Id AND sl.IsDeleted = 0
                {moFilter}";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<SalesLeadDto>(sql, parameters);

            if (dto != null)
            {
                if (dto.PartyId.HasValue)
                {
                    var party = await _partyLookup.GetByIdAsync(dto.PartyId.Value);
                    dto.PartyName = party?.PartyName;
                }

                if (dto.CityId.HasValue)
                {
                    var city = await _cityLookup.GetByIdAsync(dto.CityId.Value);
                    dto.CityName = city?.CityName;
                }

                if (dto.ItemId.HasValue)
                {
                    var items = await _itemLookup.GetByIdsAsync(new[] { dto.ItemId.Value });
                    dto.ItemName = items.FirstOrDefault()?.ItemName;
                }

                // MarketingOfficerName is populated via SQL JOIN — no cross-module lookup needed
            }

            return dto;
        }

        public async Task<IReadOnlyList<SalesLeadLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            // Marketing Officer access scoping
            var moFilter = "";
            var parameters = new DynamicParameters();
            parameters.Add("Term", $"%{term}%");

            if (_accessFilter.IsMarketingOfficer())
            {
                var empId = _accessFilter.GetCurrentMarketingOfficerId();
                var userId = _ipAddressService.GetUserId();
                var customerIds = await _accessFilter.GetAccessibleCustomerIdsAsync(ct);
                var safeIds = customerIds.Count > 0 ? customerIds.ToArray() : new[] { -1 };

                moFilter = " AND (sl.CreatedBy = @UserId OR sl.MarketingOfficerId = @EmpId OR sl.PartyId IN @CustomerIds) ";
                parameters.Add("UserId", userId);
                parameters.Add("EmpId", empId);
                parameters.Add("CustomerIds", safeIds);
            }

            var sql = $@"
                SELECT  sl.Id, sl.ContactName, sl.MobileNumber
                FROM Sales.SalesLead sl
                WHERE sl.IsDeleted = 0 AND sl.IsActive = 1
                AND (sl.ContactName LIKE @Term OR sl.MobileNumber LIKE @Term)
                {moFilter}
                ORDER BY sl.ContactName ASC";

            var rows = (await _dbConnection.QueryAsync<SalesLeadLookupDto>(
                new CommandDefinition(sql, parameters, cancellationToken: ct))).ToList();

            return rows;
        }

        public async Task<bool> MobileNumberExistsForProspectAsync(string mobileNumber, int? excludeId = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesLead
                WHERE MobileNumber = @MobileNumber
                AND PartyId IS NULL
                AND IsDeleted = 0";

            if (excludeId.HasValue && excludeId.Value > 0)
                sql += " AND Id != @ExcludeId";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql,
                new { MobileNumber = mobileNumber.Trim(), ExcludeId = excludeId });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesLead
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> ContactExistsAsync(int contactId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesContact
                WHERE Id = @ContactId AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { ContactId = contactId });
            return count > 0;
        }

        public async Task<bool> LeadSourceExistsAsync(int leadSourceId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MiscMaster
                WHERE Id = @LeadSourceId AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { LeadSourceId = leadSourceId });
            return count > 0;
        }

        public async Task<bool> PartyExistsAsync(int partyId)
        {
            var party = await _partyLookup.GetByIdAsync(partyId);
            return party != null;
        }

        public async Task<bool> CityExistsAsync(int cityId)
        {
            var city = await _cityLookup.GetByIdAsync(cityId);
            return city != null;
        }

        public async Task<bool> ItemExistsAsync(int itemId)
        {
            var items = await _itemLookup.GetByIdsAsync(new[] { itemId });
            return items.Any();
        }

        public async Task<bool> MarketingOfficerExistsAsync(int marketingOfficerId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MarketingOfficer
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = marketingOfficerId });
            return count > 0;
        }

        public async Task<bool> MobileNumberExistsInSalesContactAsync(string mobileNumber)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesContact
                WHERE MobileNumber = @MobileNumber AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { MobileNumber = mobileNumber.Trim() });
            return count > 0;
        }

        public async Task<int> GetPrimaryContactTypeIdAsync()
        {
            const string sql = @"
                SELECT TOP 1 mm.Id
                FROM Sales.MiscMaster mm
                WHERE mm.Description = 'Primary'
                AND mm.IsActive = 1 AND mm.IsDeleted = 0
                ORDER BY mm.SortOrder ASC";

            return await _dbConnection.ExecuteScalarAsync<int>(sql);
        }
    }
}
