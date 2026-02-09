using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.Common.Interfaces.IMiscTypeMaster;
using Dapper;

namespace ProjectManagement.Infrastructure.Repositories.MiscTypeMaster
{
    public class MiscTypeMasterQueryRepository : IMiscTypeMasterQueryRepository
    {
            private readonly IDbConnection _dbConnection;


        public MiscTypeMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            
        }
        public async Task<(List<ProjectManagement.Domain.Entities.MiscTypeMaster>,int)> GetAllMiscTypeMasterAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
                 var query = $$"""
                    DECLARE @TotalCount INT;
                    SELECT @TotalCount = COUNT(*) 
                    FROM [Project].[MiscTypeMaster] M
                    WHERE M.IsDeleted = 0
                    {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.MiscTypeCode LIKE @Search)")}}; 

                    SELECT M.Id, M.MiscTypeCode, M.Description, M.IsActive, M.IsDeleted, M.CreatedBy, 
                        M.CreatedDate, M.CreatedByName, M.CreatedIP, M.ModifiedBy, M.ModifiedDate, 
                        M.ModifiedByName, M.ModifiedIP  
                    FROM Project.MiscTypeMaster M
                    WHERE M.IsDeleted = 0 
                    {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.MiscTypeCode LIKE @Search)")}}
                    ORDER BY M.Id DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                    SELECT @TotalCount AS TotalCount;
                    """;
                            var parameters = new
                       {
                           Search = $"%{SearchTerm}%",
                           Offset = (PageNumber - 1) * PageSize,
                           PageSize
                       };
            var misctype = await _dbConnection.QueryMultipleAsync(query, parameters);
            var misctypemaster = (await misctype.ReadAsync<ProjectManagement.Domain.Entities.MiscTypeMaster>()).ToList();
            int totalCount = (await misctype.ReadFirstAsync<int>());
            return (misctypemaster, totalCount);
        }
          public async Task<ProjectManagement.Domain.Entities.MiscTypeMaster?> GetByMiscTypeMasterCodeAsync(string name, int? id = null)
        {
              var query = """
                 SELECT * FROM Project.MiscTypeMaster
                 WHERE MiscTypeCode = @Name AND IsDeleted = 0
                 """;

             var parameters = new DynamicParameters(new { Name = name });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }

            return await _dbConnection.QueryFirstOrDefaultAsync<ProjectManagement.Domain.Entities.MiscTypeMaster>(query, parameters);
        } 

            public async Task<ProjectManagement.Domain.Entities.MiscTypeMaster> GetByIdAsync(int id)
        {            
           const string query = @" SELECT Id,MiscTypeCode,Description,IsActive  FROM Project.MiscTypeMaster          
             WHERE Id = @id AND IsDeleted = 0";                          
            return await _dbConnection.QueryFirstOrDefaultAsync<ProjectManagement.Domain.Entities.MiscTypeMaster>(query, new { id }) ?? new ProjectManagement.Domain.Entities.MiscTypeMaster();
        }
          public async Task<List<ProjectManagement.Domain.Entities.MiscTypeMaster>>  GetMiscTypeMaster(string searchPattern)
        {
            

            const string query = @"SELECT Id, MiscTypeCode   FROM Project.MiscTypeMaster
                WHERE IsDeleted = 0 AND MiscTypeCode LIKE @SearchPattern ";
                
            
            var parameters = new 
              { 
                  SearchPattern = $"%{searchPattern ?? string.Empty}%", 
               
              };

            var misctypemaster = await _dbConnection.QueryAsync<ProjectManagement.Domain.Entities.MiscTypeMaster>(query, parameters);
            return misctypemaster.ToList();
        }
          public async Task<bool> AlreadyExistsAsync(string miscTypeCode, int? id = null)
        {   

              var query = "SELECT COUNT(1) FROM Project.MiscTypeMaster WHERE MiscTypeCode = @miscTypeCode AND IsDeleted = 0";
                var parameters = new DynamicParameters(new { MiscTypeCode = miscTypeCode });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
                return count > 0;
        }

         public async Task<bool> NotFoundAsync(int id)
        {
            var query = "SELECT COUNT(1) FROM  Project.MiscTypeMaster WHERE Id = @Id AND IsDeleted = 0";
             
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

         public async Task<bool> SoftDeleteValidation(int Id)
        {
            const string query = @"
                           SELECT 1 
                           FROM  Project.MiscTypeMaster
                           WHERE Id  = @Id AND IsDeleted = 0;";
                    
            using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });
                    
            var shiftMasterDetailExists = await multi.ReadFirstOrDefaultAsync<int?>();
                    
            return shiftMasterDetailExists.HasValue;
        }

      
    }
}