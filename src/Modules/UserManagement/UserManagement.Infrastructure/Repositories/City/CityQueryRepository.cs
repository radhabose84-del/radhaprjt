using Core.Domain.Entities;
using System.Data;
using Dapper;
using Core.Application.Common.Interfaces.ICity;

namespace UserManagement.Infrastructure.Repositories.City
{
    public class CityQueryRepository : ICityQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        public CityQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<(List<Cities>, int)> GetAllCityAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM AppData.City 
                WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (CityCode LIKE @Search OR CityName LIKE @Search)")}};

                SELECT Id,CityCode,CityName,IsActive,StateId,CreatedBy,CreatedAt,CreatedByName,CreatedIP,ModifiedBy,ModifiedAt,ModifiedByName,ModifiedIP
                FROM AppData.City WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (CityCode LIKE @Search OR CityName LIKE @Search )")}}
                ORDER BY Id desc
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                SELECT @TotalCount AS TotalCount;
                """;
            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

            var cities = await _dbConnection.QueryMultipleAsync(query, parameters);
            var cityList = (await cities.ReadAsync<Cities>()).ToList();
            int totalCount = (await cities.ReadFirstAsync<int>());
            return (cityList, totalCount);
        }
        public async Task<Cities> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT Id, CityCode, CityName, IsActive,StateId,CreatedBy,CreatedAt,CreatedByName,CreatedIP,ModifiedBy,ModifiedAt,ModifiedByName,ModifiedIP 
                FROM AppData.City  WHERE Id = @Id AND IsDeleted=0";
            var city = await _dbConnection.QueryFirstOrDefaultAsync<Cities>(query, new { id });
            if (city is null)
            {
                throw new KeyNotFoundException($"City with ID {id} not found.");
            }
            return city;
        }
        public async Task<List<Cities>> GetByCityNameAsync(string searchPattern)
        {
            const string query = @"
                SELECT Id, CityCode, CityName,StateId, IsActive, CreatedBy, CreatedAt, CreatedByName, CreatedIP, 
                ModifiedBy, ModifiedAt, ModifiedByName, ModifiedIP
                FROM AppData.City
                WHERE (CityName LIKE @SearchPattern OR CityCode LIKE @SearchPattern) 
                AND  IsDeleted=0 and IsActive=1
                ORDER BY ID DESC";
            var result = await _dbConnection.QueryAsync<Cities>(query, new { SearchPattern = $"%{searchPattern}%" });
            return result.ToList();
        }
        public async Task<List<Cities>> GetCityByStateIdAsync(int stateId)
        {
            const string query = @"
                SELECT Id, CityCode, CityName,StateId, IsActive, CreatedBy, CreatedAt, CreatedByName, CreatedIP, 
                ModifiedBy, ModifiedAt, ModifiedByName, ModifiedIP
                FROM AppData.City WHERE StateId = @StateId  and IsDeleted=0";
            var result = await _dbConnection.QueryAsync<Cities>(query, new { stateId });
            if (result is null)
            {
                throw new KeyNotFoundException($"State with ID {stateId} not found.");
            }
            return result.ToList();
        }
        public async Task<bool> SoftDeleteValidation(int id)
        {
            const string query = @"
        SELECT TOP 1 1
        FROM [AppData].[Company] C
        INNER JOIN [AppData].[CompanyAddress] CA ON C.Id = CA.CompanyId
        WHERE CA.CityId = @Id
          AND C.IsDeleted = 0;

        SELECT TOP 1 1
        FROM [AppData].[Unit] U
        INNER JOIN [AppData].[UnitAddress] UA ON U.Id = UA.UnitId
        WHERE UA.CityId = @Id
          AND U.IsDeleted = 0;
    ";

            using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = id });

            var companyUsed = await multi.ReadFirstOrDefaultAsync<int?>();
            var unitUsed = await multi.ReadFirstOrDefaultAsync<int?>();

            return companyUsed.HasValue || unitUsed.HasValue;
        }

        public async Task<bool> IsCityLinkedAsync(int cityId)
        {
        const string companyAddressQuery = @"
        SELECT TOP 1 1
        FROM [AppData].[CompanyAddress]
        WHERE CityId = @cityId;
        ";

        const string unitAddressQuery = @"
        SELECT TOP 1 1
        FROM [AppData].[UnitAddress]
        WHERE CityId = @cityId;
        ";

            var companyLinked = await _dbConnection.QueryFirstOrDefaultAsync<int?>(companyAddressQuery, new { cityId });
            if (companyLinked.HasValue)
                return true;

            var unitLinked = await _dbConnection.QueryFirstOrDefaultAsync<int?>(unitAddressQuery, new { cityId });
            return unitLinked.HasValue;
        }


    }
}