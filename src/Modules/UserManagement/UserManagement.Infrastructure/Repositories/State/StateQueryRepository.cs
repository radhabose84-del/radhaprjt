using UserManagement.Domain.Entities;
using System.Data;
using Dapper;
using UserManagement.Application.Common;
using UserManagement.Application.Common.Interfaces.IState;

namespace UserManagement.Infrastructure.Repositories
{    
    public class StateQueryRepository : IStateQueryRepository
    {        
        private readonly IDbConnection _dbConnection;

        public StateQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<(List<States>, int)> GetAllStatesAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
             var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM AppData.State 
                WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (StateCode LIKE @Search OR StateName LIKE @Search)")}};

                SELECT Id,StateCode,StateName,IsActive,CountryId,CreatedBy,CreatedAt,CreatedByName,CreatedIP,ModifiedBy,ModifiedAt,ModifiedByName,ModifiedIP
                FROM AppData.State WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (StateCode LIKE @Search OR StateName LIKE @Search )")}}
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

          
            var states = await _dbConnection.QueryMultipleAsync(query, parameters);
            var stateList = (await states.ReadAsync<States>()).ToList();
            int totalCount = (await states.ReadFirstAsync<int>());             
            return (stateList, totalCount); 
        }

        public async Task<States> GetByIdAsync(int id)
        {
            const string query = @"
            SELECT Id, StateCode, StateName, IsActive,countryId,CreatedBy,CreatedAt,CreatedByName,CreatedIP,ModifiedBy,ModifiedAt,ModifiedByName,ModifiedIP 
            FROM AppData.State WHERE Id = @Id and IsDeleted=0";
            var state = await _dbConnection.QueryFirstOrDefaultAsync<States>(query, new { id });           
            if (state is null)
            {
                throw new KeyNotFoundException($"State with ID {id} not found.");
            }
            return state;
        }

        public async Task<List<States>> GetByStateNameAsync(string searchPattern)
        {                      
            const string query = @"
                SELECT Id, StateCode, StateName,countryId, IsActive, CreatedBy, CreatedAt, CreatedByName, CreatedIP, 
                ModifiedBy, ModifiedAt, ModifiedByName, ModifiedIP
                FROM AppData.State 
                WHERE (StateName LIKE @SearchPattern OR StateCode LIKE @SearchPattern) 
                AND IsDeleted = 0 and IsActive=1
                ORDER BY ID DESC";
            var result = await _dbConnection.QueryAsync<States>(query, new { SearchPattern = $"%{searchPattern}%" });
            return result.ToList();              
        }

        public async Task<List<States>> GetCityByStateAsync(int stateId)
        {
            const string query = @"
            SELECT Id, CityCode, CityName,StateId, IsActive, CreatedBy, CreatedAt, CreatedByName, CreatedIP, 
            ModifiedBy, ModifiedAt, ModifiedByName, ModifiedIP
            FROM AppData.City WHERE StateId = @StateId  and IsDeleted=0";
            var state = await _dbConnection.QueryAsync<States>(query, new { stateId });           
            if (state is null)
            {
                throw new KeyNotFoundException($"State with ID {stateId} not found.");
            }
            return state.ToList();
        }

        public async Task<List<States>> GetStateByCountryIdAsync(int countryId )
        {
           const string query = @"
            SELECT Id, StateCode, StateName, IsActive,countryId,CreatedBy,CreatedAt,CreatedByName,CreatedIP,ModifiedBy,ModifiedAt,ModifiedByName,ModifiedIP 
            FROM AppData.State WHERE CountryId = @CountryId  AND IsDeleted=0";
            var state = await _dbConnection.QueryAsync<States>(query, new { countryId });           
            if (state is null)
            {
                throw new KeyNotFoundException($"State with ID {countryId} not found.");
            }
            return state.ToList();
        }   
        public async Task<bool>SoftDeleteValidation(int Id)
        {
            const string query = @"
                    SELECT * 
                    FROM [AppData].[City] 
                    WHERE StateId = @Id AND IsDeleted = 1";
                    var cities = await _dbConnection.QueryFirstOrDefaultAsync<UserManagement.Domain.Entities.Cities>(query, new { Id });

            if (cities != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> IsLinkedWithCitiesAsync(int stateId)
        {
            const string query = @"
        SELECT TOP 1 1
        FROM [AppData].[City]
        WHERE IsDeleted = 0 AND StateId = @stateId;
        ";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int?>(query, new { stateId });
            return result.HasValue;
        }
    }
}
