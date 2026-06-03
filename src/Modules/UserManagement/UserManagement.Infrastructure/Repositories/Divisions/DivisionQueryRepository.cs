#nullable disable
using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces.IDivision;
using System.Data;
using Dapper;
using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Divisions.Queries.GetUnitsByDivision;

namespace UserManagement.Infrastructure.Repositories.Divisions
{
    public class DivisionQueryRepository : IDivisionQueryRepository
    {
        private readonly IDbConnection _dbConnection;        
        private readonly IIPAddressService _ipAddressService; 
        public DivisionQueryRepository(IDbConnection dbConnection,IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }
         public async Task<(List<Division>,int)> GetAllDivisionAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            var CompanyId = _ipAddressService.GetCompanyId() ?? 0;
                 var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM AppData.Division 
              WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ShortName LIKE @Search OR Name LIKE @Search)")}};

                SELECT 
                Id, 
                ShortName,
                Name,
                CompanyId,
                IsActive,
                 CreatedAt,
                CreatedByName,
                CreatedIP,
                 ModifiedAt,
                ModifiedByName,
                ModifiedIP
            FROM AppData.Division 
            WHERE 
            IsDeleted = 0 AND CompanyId=@CompanyId
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ShortName LIKE @Search OR Name LIKE @Search )")}}
                ORDER BY Id desc
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            
             var parameters = new
                       {
                           Search = $"%{SearchTerm}%",
                           Offset = (PageNumber - 1) * PageSize,
                           PageSize,
                           CompanyId
                       };

               var division = await _dbConnection.QueryMultipleAsync(query, parameters);
             var divisionlist = (await division.ReadAsync<Division>()).ToList();
             int totalCount = (await division.ReadFirstAsync<int>());
            return (divisionlist, totalCount);
        }   
        public async Task<Division> GetByDivisionnameAsync(string name, int? id = null)
        {
            var CompanyId = _ipAddressService.GetCompanyId() ?? 0;
              var query = """
                 SELECT * FROM AppData.Division 
                 WHERE Name = @Name AND IsDeleted = 0 AND CompanyId=@CompanyId
                 """;

             var parameters = new DynamicParameters(new { Name = name,CompanyId=CompanyId });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }

            return await _dbConnection.QueryFirstOrDefaultAsync<Division>(query, parameters);
        } 

         public async Task<Division> GetByIdAsync(int id)
        {
            // Look up by primary key only — do NOT company-scope a by-id fetch (consistent with
            // Unit/Company/Language GetById). Company scoping belongs on list/search, not PK lookup.
             const string query = "SELECT * FROM AppData.Division WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<Division>(query, new { id });
        }
      
        public async Task<List<Division>>  GetDivision(string searchPattern)
        {
            var CompanyId = _ipAddressService.GetCompanyId() ?? 0;
            var userId = _ipAddressService.GetUserId();

            var query = $@"
        SELECT D.Id, D.Name 
        FROM AppData.Division D
        INNER JOIN [AppSecurity].[UserDivision] UD ON UD.DivisionId=D.Id AND UD.IsActive=1
        WHERE D.IsDeleted = 0 
        AND D.Name LIKE @SearchPattern
        AND D.CompanyId=@CompanyId AND UD.UserId=@UserId";
                
            
            var parameters = new 
              { 
                  SearchPattern = $"%{searchPattern ?? string.Empty}%",
                  CompanyId =CompanyId,
                  UserId =userId
              };

            var divisions = await _dbConnection.QueryAsync<Division>(query, parameters);
            return divisions.ToList();
        }
        public async Task<List<GetUnitsByDivisionDto>> GetUnitsByDivisionAsync(int companyId, int divisionId)
        {
            const string sql = @"
                SELECT
                    U.CompanyId,
                    U.Id        AS UnitId,
                    U.UnitName,
                    U.UnitTypeId,
                    MM.Description AS UnitTypeName,
                    D.Id        AS DivisionId,
                    D.Name      AS DivisionName
                FROM [AppData].[Unit] U
                INNER JOIN [AppData].[Division] D ON D.Id = U.DivisionId AND D.IsDeleted = 0
                LEFT JOIN [AppData].[MiscMaster] MM ON MM.Id = U.UnitTypeId AND MM.IsDeleted = 0
                WHERE U.IsDeleted = 0
                  AND U.CompanyId  = @CompanyId
                  AND U.DivisionId = @DivisionId
                ORDER BY U.UnitName ASC;";

            var result = await _dbConnection.QueryAsync<GetUnitsByDivisionDto>(sql, new { CompanyId = companyId, DivisionId = divisionId });
            return result.ToList();
        }

           public async Task<bool>SoftDeleteValidation(int Id)
           {
                                const string query = @"
                           SELECT 1 
                           FROM [AppData].[Unit] 
                           WHERE DivisionId = @Id AND IsDeleted = 0;";
                    
                       using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });
                    
                       
                       var divisionExists = await multi.ReadFirstOrDefaultAsync<int?>();
                    
                       return divisionExists.HasValue ;
          }
             public async Task<bool> FKColumnExistValidation(int Id)
          {
              var sql = "SELECT COUNT(1) FROM AppData.Division WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";
                var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = Id });
                return count > 0;
          }
            public async Task<List<Division>>  GetDivision_SuperAdmin(string searchPattern)
            {
                var CompanyId = _ipAddressService.GetCompanyId() ?? 0;

                var query = $@"
                 SELECT Id, Name
                 FROM AppData.Division
                 WHERE IsDeleted = 0
                 AND Name LIKE @SearchPattern
                 AND CompanyId=@CompanyId";


                var parameters = new
                  {
                      SearchPattern = $"%{searchPattern ?? string.Empty}%",
                      CompanyId =CompanyId
                  };

                var divisions = await _dbConnection.QueryAsync<Division>(query, parameters);
                return divisions.ToList();
            }

        /// <inheritdoc />
        public async Task<bool> IsDivisionLinkedAsync(int divisionId)
        {
            // Same-module: check if any active, non-deleted Unit references this division
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [AppData].[Unit] WHERE DivisionId = @Id AND IsDeleted = 0 AND IsActive = 1)
                THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = divisionId });
        }
    }
}