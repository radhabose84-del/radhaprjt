using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Dto;

namespace SalesManagement.Infrastructure.Repositories.SalesGroup
{
    public class SalesGroupQueryRepository : ISalesGroupQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IInventoryCategoryLookup _categoryLookup;

        public SalesGroupQueryRepository(IDbConnection dbConnection, IInventoryCategoryLookup categoryLookup)
        {
            _dbConnection = dbConnection;
            _categoryLookup = categoryLookup;
        }

        public async Task<(List<SalesGroupDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesGroup sg
                INNER JOIN Sales.SalesOffice so ON sg.SalesOfficeId = so.Id
                WHERE sg.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (sg.SalesGroupName LIKE @Search)")}};

                SELECT sg.Id, sg.SalesGroupName, sg.SalesOfficeId,
                    so.SalesOfficeName,
                    sg.ResponsibleManager, sg.ProductCategoryId, sg.RegionTerritory,
                    sg.IsActive, sg.IsDeleted,
                    sg.CreatedBy, sg.CreatedDate, sg.CreatedByName, sg.CreatedIP,
                    sg.ModifiedBy, sg.ModifiedDate, sg.ModifiedByName, sg.ModifiedIP
                FROM Sales.SalesGroup sg
                INNER JOIN Sales.SalesOffice so ON sg.SalesOfficeId = so.Id
                WHERE sg.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (sg.SalesGroupName LIKE @Search)")}}
                ORDER BY sg.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<SalesGroupDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Any())
            {
                var categoryIds = list
                    .Where(x => x.ProductCategoryId.HasValue)
                    .Select(x => x.ProductCategoryId!.Value)
                    .Distinct()
                    .ToList();

                if (categoryIds.Any())
                {
                    var categories = await _categoryLookup.GetCategoryByIdsAsync(categoryIds);
                    var categoryDict = categories.ToDictionary(c => c.Id, c => c.ItemCategoryName);

                    foreach (var item in list.Where(x => x.ProductCategoryId.HasValue))
                    {
                        item.ProductCategoryName = categoryDict.TryGetValue(item.ProductCategoryId!.Value, out var name) ? name : null;
                    }
                }
            }

            return (list, totalCount);
        }

        public async Task<SalesGroupDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT sg.Id, sg.SalesGroupName, sg.SalesOfficeId,
                    so.SalesOfficeName,
                    sg.ResponsibleManager, sg.ProductCategoryId, sg.RegionTerritory,
                    sg.IsActive, sg.IsDeleted,
                    sg.CreatedBy, sg.CreatedDate, sg.CreatedByName, sg.CreatedIP,
                    sg.ModifiedBy, sg.ModifiedDate, sg.ModifiedByName, sg.ModifiedIP
                FROM Sales.SalesGroup sg
                INNER JOIN Sales.SalesOffice so ON sg.SalesOfficeId = so.Id
                WHERE sg.Id = @Id AND sg.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<SalesGroupDto>(sql, new { Id = id });

            if (dto?.ProductCategoryId.HasValue == true)
            {
                var categories = await _categoryLookup.GetCategoryByIdsAsync(new[] { dto.ProductCategoryId!.Value });
                dto.ProductCategoryName = categories?.FirstOrDefault()?.ItemCategoryName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<SalesGroupLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20 Id, SalesGroupName, SalesOfficeId
                FROM Sales.SalesGroup
                WHERE IsDeleted = 0 AND IsActive = 1
                AND SalesGroupName LIKE @Term
                ORDER BY SalesGroupName ASC";

            var result = await _dbConnection.QueryAsync<SalesGroupLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string salesGroupName, int salesOfficeId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesGroup
                WHERE SalesGroupName = @Name
                AND SalesOfficeId = @SalesOfficeId
                AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql,
                new { Name = salesGroupName.Trim(), SalesOfficeId = salesOfficeId, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesGroup
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> SalesOfficeExistsAsync(int salesOfficeId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesOffice
                WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesOfficeId });
            return count > 0;
        }

        public async Task<bool> ProductCategoryExistsAsync(int categoryId, CancellationToken ct = default)
        {
            var categories = await _categoryLookup.GetCategoryByIdsAsync(new[] { categoryId }, ct);
            return categories != null && categories.Any();
        }
    }
}
