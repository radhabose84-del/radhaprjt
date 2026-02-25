using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Dto;

namespace SalesManagement.Infrastructure.Repositories.SalesOffice
{
    public class SalesOfficeQueryRepository : ISalesOfficeQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICityLookup _cityLookup;

        public SalesOfficeQueryRepository(IDbConnection dbConnection, ICityLookup cityLookup)
        {
            _dbConnection = dbConnection;
            _cityLookup = cityLookup;
        }

        public async Task<(List<SalesOfficeDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesOffice so
                INNER JOIN Sales.SalesOrganisation org ON so.SalesOrganisationId = org.Id
                WHERE so.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (so.SalesOfficeName LIKE @Search)")}};

                SELECT so.Id, so.SalesOfficeName, so.SalesOrganisationId,
                    org.SalesOrganisationName,
                    so.CityId, so.Pincode, so.Phone, so.Email,
                    so.ResponsibleManager, so.RegionTerritory, so.Address,
                    so.IsActive, so.IsDeleted,
                    so.CreatedBy, so.CreatedDate, so.CreatedByName, so.CreatedIP,
                    so.ModifiedBy, so.ModifiedDate, so.ModifiedByName, so.ModifiedIP
                FROM Sales.SalesOffice so
                INNER JOIN Sales.SalesOrganisation org ON so.SalesOrganisationId = org.Id
                WHERE so.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (so.SalesOfficeName LIKE @Search)")}}
                ORDER BY so.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<SalesOfficeDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Any())
            {
                var cities = await _cityLookup.GetAllCityAsync();
                var cityDict = cities.ToDictionary(c => c.CityId, c => c.CityName);

                foreach (var item in list)
                {
                    item.CityName = cityDict.TryGetValue(item.CityId, out var name) ? name : null;
                }
            }

            return (list, totalCount);
        }

        public async Task<SalesOfficeDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT so.Id, so.SalesOfficeName, so.SalesOrganisationId,
                    org.SalesOrganisationName,
                    so.CityId, so.Pincode, so.Phone, so.Email,
                    so.ResponsibleManager, so.RegionTerritory, so.Address,
                    so.IsActive, so.IsDeleted,
                    so.CreatedBy, so.CreatedDate, so.CreatedByName, so.CreatedIP,
                    so.ModifiedBy, so.ModifiedDate, so.ModifiedByName, so.ModifiedIP
                FROM Sales.SalesOffice so
                INNER JOIN Sales.SalesOrganisation org ON so.SalesOrganisationId = org.Id
                WHERE so.Id = @Id AND so.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<SalesOfficeDto>(sql, new { Id = id });

            if (dto != null)
            {
                var city = await _cityLookup.GetByIdAsync(dto.CityId);
                dto.CityName = city?.CityName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<SalesOfficeLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20 Id, SalesOfficeName, SalesOrganisationId
                FROM Sales.SalesOffice
                WHERE IsDeleted = 0 AND IsActive = 1
                AND SalesOfficeName LIKE @Term
                ORDER BY SalesOfficeName ASC";

            var result = await _dbConnection.QueryAsync<SalesOfficeLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string salesOfficeName, int salesOrganisationId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesOffice
                WHERE SalesOfficeName = @Name
                AND SalesOrganisationId = @SalesOrganisationId
                AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Name = salesOfficeName.Trim(), SalesOrganisationId = salesOrganisationId, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesOffice
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> SalesOrganisationExistsAsync(int salesOrganisationId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesOrganisation
                WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesOrganisationId });
            return count > 0;
        }

        public async Task<bool> CityExistsAsync(int cityId)
        {
            var city = await _cityLookup.GetByIdAsync(cityId);
            return city != null;
        }
    }
}
