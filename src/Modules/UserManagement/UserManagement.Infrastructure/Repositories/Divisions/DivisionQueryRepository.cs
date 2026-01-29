using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IDivision;
using Core.Application.Divisions.Queries.GetDivisions;
using Core.Application.Common;
using System.Data;
using Dapper;
using Core.Application.Common.Interfaces;

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
         public async Task<(List<Division>,int)> GetAllDivisionAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var CompanyId = _ipAddressService.GetCompanyId();
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
        public async Task<Division?> GetByDivisionnameAsync(string name, int? id = null)
        {
            var CompanyId = _ipAddressService.GetCompanyId();
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
            
                var CompanyId = _ipAddressService.GetCompanyId();
             const string query = "SELECT * FROM AppData.Division WHERE Id = @Id AND IsDeleted = 0 AND CompanyId=@CompanyId";
            return await _dbConnection.QueryFirstOrDefaultAsync<Division>(query, new { id,CompanyId });
        }
      
        public async Task<List<Division>>  GetDivision(string searchPattern)
        {
            var CompanyId = _ipAddressService.GetCompanyId();
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
                var CompanyId = _ipAddressService.GetCompanyId();

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
    }
}