using Core.Domain.Entities;
using System.Data;
using Dapper;
using Core.Application.Common.Interfaces.ICountry;

namespace UserManagement.Infrastructure.Repositories.Country
{    
    public class CountryQueryRepository : ICountryQueryRepository
    {        
        private readonly IDbConnection _dbConnection;
        
        public CountryQueryRepository(IDbConnection dbConnection)
        {            
            _dbConnection = dbConnection;
        }
        public async Task<(List<Countries>, int)> GetAllCountriesAsync(int PageNumber, int PageSize, string? SearchTerm)
        {

               var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM AppData.Country 
                WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (CountryCode LIKE @Search OR CountryName LIKE @Search)")}};

                SELECT Id,CountryCode, CountryName, IsActive ,CreatedBy,CreatedAt,CreatedByName,CreatedIP,ModifiedBy,ModifiedAt,ModifiedByName,ModifiedIP
                FROM AppData.Country WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (CountryCode LIKE @Search OR CountryName LIKE @Search )")}}
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

          
            var countries = await _dbConnection.QueryMultipleAsync(query, parameters);
            var countryList = (await countries.ReadAsync<Countries>()).ToList();
            int totalCount = (await countries.ReadFirstAsync<int>());             
            return (countryList, totalCount);    
        }

        public async Task<Countries> GetByIdAsync(int id)
        {          

            const string query = @"
            SELECT Id, CountryCode, CountryName, IsActive, CreatedBy, CreatedAt, CreatedByName, CreatedIP, 
            ModifiedBy, ModifiedAt, ModifiedByName, ModifiedIP
            FROM AppData.Country
            WHERE Id = @id AND IsDeleted = 0";
            var country = await _dbConnection.QueryFirstOrDefaultAsync<Countries>(query, new { id });           
             if (country is null)
            {
                throw new KeyNotFoundException($"Country with ID {id} not found.");
            }
            return country;
        }
        public async Task<List<Countries>> GetByCountryNameAsync(string searchPattern)
        {           
            const string query = @"
                SELECT Id, countryCode, countryName, IsActive, CreatedBy, CreatedAt, CreatedByName, CreatedIP, 
                ModifiedBy, ModifiedAt, ModifiedByName, ModifiedIP
                FROM AppData.Country
                WHERE (CountryName LIKE @SearchPattern OR CountryCode LIKE @SearchPattern) 
                AND IsDeleted = 0 and IsActive=1
                ORDER BY ID DESC";
            var result = await _dbConnection.QueryAsync<Countries>(query, new { SearchPattern = $"%{searchPattern}%" });
            return result.ToList();
        }

        public async Task<List<Countries>> GetStateByCountryIdAsync(int countryId)
        {
            const string query = @"
            SELECT Id, StateCode, StateName, IsActive,countryId,CreatedBy,CreatedAt,CreatedByName,CreatedIP,ModifiedBy,ModifiedAt,ModifiedByName,ModifiedIP 
            FROM AppData.State WHERE CountryId = @CountryId  AND IsDeleted=0";
            var state = await _dbConnection.QueryAsync<Countries>(query, new { countryId });           
            if (state is null)
            {
                throw new KeyNotFoundException($"State with ID {countryId} not found.");
            }
            return state.ToList();
        }
       
          public async Task<bool>SoftDeleteValidation(int Id)
            {
                                const string query = @"
                           SELECT 1 
                           FROM [AppData].[State] 
                         WHERE CountryId = @Id AND   IsDeleted = 0;
                         
                         SELECT 1 
                           FROM [AppData].[Company] C
                           INNER JOIN [AppData].[CompanyAddress] CA ON C.Id=CA.CompanyId 
                         WHERE CA.CountryId = @Id AND   C.IsDeleted = 0;";

            using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });

            var StateExists = await multi.ReadFirstOrDefaultAsync<int?>();
            var CompanyExists = await multi.ReadFirstOrDefaultAsync<int?>();

            return StateExists.HasValue || CompanyExists.HasValue;
        }

        public async Task<bool> IsLinkedWithStatesAsync(int countryId)
        {
            const string query = @"
        SELECT TOP 1 1
        FROM [AppData].[State]
        WHERE IsDeleted = 0 AND CountryId = @countryId;
    ";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int?>(query, new { countryId });
            return result.HasValue;
        }              
    }
}