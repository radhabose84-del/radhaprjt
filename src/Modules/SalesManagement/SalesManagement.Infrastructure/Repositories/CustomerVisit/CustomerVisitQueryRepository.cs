using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Application.CustomerVisit.Dto;
using SalesManagement.Domain.Common;

namespace SalesManagement.Infrastructure.Repositories.CustomerVisit
{
    public class CustomerVisitQueryRepository : ICustomerVisitQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyLookup _companyLookup;
        private readonly IUnitLookup _unitLookup;

        public CustomerVisitQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IItemLookup itemLookup,
            IIPAddressService ipAddressService,
            ICompanyLookup companyLookup,
            IUnitLookup unitLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
            _ipAddressService = ipAddressService;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
        }

        public async Task<(List<CustomerVisitDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string countSql = @"
                SELECT COUNT(*)
                FROM Sales.CustomerVisit cv
                LEFT JOIN Sales.MiscMaster mm ON cv.VisitTypeId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.MarketingOfficer mo ON cv.MarketingOfficerId = mo.Id AND mo.IsDeleted = 0
                WHERE cv.IsDeleted = 0
                AND (@SearchTerm IS NULL OR @SearchTerm = ''
                     OR mm.Description LIKE '%' + @SearchTerm + '%'
                     OR mo.EmployeeName LIKE '%' + @SearchTerm + '%'
                     OR cv.Remarks LIKE '%' + @SearchTerm + '%');";

            const string dataSql = @"
                SELECT
                    cv.Id, cv.CustomerId, cv.VisitTypeId, cv.VisitDateTime,
                    cv.Latitude, cv.Longitude, cv.ImageName, cv.Remarks,
                    cv.MarketingOfficerId,
                    cv.IsActive, cv.IsDeleted,
                    cv.CreatedBy, cv.CreatedDate, cv.CreatedByName, cv.CreatedIP,
                    cv.ModifiedBy, cv.ModifiedDate, cv.ModifiedByName, cv.ModifiedIP,
                    mm.Description AS VisitTypeName,
                    mo.EmployeeName AS MarketingOfficerName
                FROM Sales.CustomerVisit cv
                LEFT JOIN Sales.MiscMaster mm ON cv.VisitTypeId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.MarketingOfficer mo ON cv.MarketingOfficerId = mo.Id AND mo.IsDeleted = 0
                WHERE cv.IsDeleted = 0
                AND (@SearchTerm IS NULL OR @SearchTerm = ''
                     OR mm.Description LIKE '%' + @SearchTerm + '%'
                     OR mo.EmployeeName LIKE '%' + @SearchTerm + '%'
                     OR cv.Remarks LIKE '%' + @SearchTerm + '%')
                ORDER BY cv.VisitDateTime DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new { SearchTerm = searchTerm, Offset = offset, PageSize = pageSize };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var list = (await _dbConnection.QueryAsync<CustomerVisitDto>(dataSql, parameters)).ToList();

            // Cross-module lookup: populate CustomerName via IPartyLookup
            var customerIds = list.Select(x => x.CustomerId).Distinct();
            if (customerIds.Any())
            {
                var parties = await _partyLookup.GetByIdsAsync(customerIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                foreach (var item in list)
                {
                    item.CustomerName = partyDict.TryGetValue(item.CustomerId, out var name) ? name : null;
                }
            }

            // Construct full image path
            var imageBasePath = await GetImageBasePathAsync();
            foreach (var item in list)
            {
                if (!string.IsNullOrWhiteSpace(item.ImageName))
                {
                    item.ImagePath = Path.Combine(imageBasePath, item.ImageName).Replace("\\", "/");
                }
            }

            return (list, totalCount);
        }

        public async Task<CustomerVisitDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT
                    cv.Id, cv.CustomerId, cv.VisitTypeId, cv.VisitDateTime,
                    cv.Latitude, cv.Longitude, cv.ImageName, cv.Remarks,
                    cv.MarketingOfficerId,
                    cv.IsActive, cv.IsDeleted,
                    cv.CreatedBy, cv.CreatedDate, cv.CreatedByName, cv.CreatedIP,
                    cv.ModifiedBy, cv.ModifiedDate, cv.ModifiedByName, cv.ModifiedIP,
                    mm.Description AS VisitTypeName,
                    mo.EmployeeName AS MarketingOfficerName
                FROM Sales.CustomerVisit cv
                LEFT JOIN Sales.MiscMaster mm ON cv.VisitTypeId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Sales.MarketingOfficer mo ON cv.MarketingOfficerId = mo.Id AND mo.IsDeleted = 0
                WHERE cv.Id = @Id AND cv.IsDeleted = 0;";

            const string detailSql = @"
                SELECT Id, ItemId
                FROM Sales.CustomerVisitProduct
                WHERE CustomerVisitId = @Id;";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<CustomerVisitDto>(headerSql, new { Id = id });

            if (dto == null)
                return null;

            // Cross-module lookup: populate CustomerName
            var party = await _partyLookup.GetByIdAsync(dto.CustomerId);
            dto.CustomerName = party?.PartyName;

            // Construct full image path
            if (!string.IsNullOrWhiteSpace(dto.ImageName))
            {
                var imageBasePath = await GetImageBasePathAsync();
                dto.ImagePath = Path.Combine(imageBasePath, dto.ImageName).Replace("\\", "/");
            }

            // Fetch detail products
            var products = (await _dbConnection.QueryAsync<CustomerVisitProductDto>(detailSql, new { Id = id })).ToList();

            // Cross-module lookup: populate ItemName/ItemCode via IItemLookup
            var itemIds = products.Select(p => p.ItemId).Distinct();
            if (itemIds.Any())
            {
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id);
                foreach (var product in products)
                {
                    if (itemDict.TryGetValue(product.ItemId, out var itemDto))
                    {
                        product.ItemName = itemDto.ItemName;
                        product.ItemCode = itemDto.ItemCode;
                    }
                }
            }

            dto.Products = products;

            return dto;
        }

        public async Task<IReadOnlyList<CustomerVisitLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20 cv.Id, cv.CustomerId, cv.VisitDateTime
                FROM Sales.CustomerVisit cv
                WHERE cv.IsActive = 1 AND cv.IsDeleted = 0
                AND (@Term = '' OR cv.Remarks LIKE '%' + @Term + '%')
                ORDER BY cv.VisitDateTime DESC;";

            var command = new CommandDefinition(sql, new { Term = term }, cancellationToken: ct);
            var list = (await _dbConnection.QueryAsync<CustomerVisitLookupDto>(command)).ToList();

            // Populate CustomerName via lookup
            var customerIds = list.Select(x => x.CustomerId).Distinct();
            if (customerIds.Any())
            {
                var parties = await _partyLookup.GetByIdsAsync(customerIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                foreach (var item in list)
                {
                    item.CustomerName = partyDict.TryGetValue(item.CustomerId, out var name) ? name : null;
                }
            }

            return list;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.CustomerVisit WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            var party = await _partyLookup.GetByIdAsync(customerId);
            return party != null;
        }

        public async Task<bool> VisitTypeExistsAsync(int visitTypeId)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.MiscMaster WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = visitTypeId });
            return count > 0;
        }

        public async Task<bool> MarketingOfficerExistsAsync(int marketingOfficerId)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.MarketingOfficer WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = marketingOfficerId });
            return count > 0;
        }

        public async Task<bool> ItemExistsAsync(int itemId)
        {
            var items = await _itemLookup.GetByIdsAsync(new[] { itemId });
            return items.Any();
        }

        public async Task<string> GetImageFolderAsync()
        {
            const string sql = @"
                SELECT Description
                FROM Sales.MiscTypeMaster
                WHERE MiscTypeCode = @MiscTypeCode AND IsDeleted = 0;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                sql, new { MiscTypeCode = MiscEnumEntity.CustomerVisitPath });
            return result ?? "CustomerVisit";
        }

        private async Task<string> GetImageBasePathAsync()
        {
            const string sql = @"
                SELECT Description
                FROM Sales.MiscTypeMaster
                WHERE MiscTypeCode = @MiscTypeCode AND IsDeleted = 0;";

            var basePath = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                sql, new { MiscTypeCode = MiscEnumEntity.CustomerVisitPath });

            if (string.IsNullOrWhiteSpace(basePath))
                return string.Empty;

            var companies = await _companyLookup.GetAllCompanyAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var companyName = companies.FirstOrDefault(c => c.CompanyId == _ipAddressService.GetCompanyId())?.CompanyName ?? string.Empty;
            var unitName = units.FirstOrDefault(u => u.UnitId == _ipAddressService.GetUnitId())?.UnitName ?? string.Empty;

            return $"{basePath.TrimEnd('/', '\\')}/{companyName}/{unitName}";
        }
    }
}
