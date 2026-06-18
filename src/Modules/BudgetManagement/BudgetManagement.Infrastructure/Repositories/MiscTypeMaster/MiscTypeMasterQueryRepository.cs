using System.Data;
using BudgetManagement.Application.Common.Interfaces.IMiscTypeMaster;
using Dapper;

namespace BudgetManagement.Infrastructure.Repositories
{
    public class MiscTypeMasterQueryRepository : IMiscTypeMasterQueryRepository
    {
            private readonly IDbConnection _dbConnection;


        public MiscTypeMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            
        }
        public async Task<(List<BudgetManagement.Domain.Entities.MiscTypeMaster>,int)> GetAllMiscTypeMasterAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
                 var query = $$"""
                    DECLARE @TotalCount INT;
                    SELECT @TotalCount = COUNT(*) 
                    FROM [Budget].[MiscTypeMaster] M
                    WHERE M.IsDeleted = 0
                    {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.MiscTypeCode LIKE @Search)")}}; 

                    SELECT M.Id, M.MiscTypeCode, M.Description, M.IsActive, M.IsDeleted, M.CreatedBy, 
                        M.CreatedDate, M.CreatedByName, M.CreatedIP, M.ModifiedBy, M.ModifiedDate, 
                        M.ModifiedByName, M.ModifiedIP  
                    FROM Budget.MiscTypeMaster M
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
            var misctypemaster = (await misctype.ReadAsync<BudgetManagement.Domain.Entities.MiscTypeMaster>()).ToList();
            int totalCount = (await misctype.ReadFirstAsync<int>());
            return (misctypemaster, totalCount);
        }
          public async Task<BudgetManagement.Domain.Entities.MiscTypeMaster?> GetByMiscTypeMasterCodeAsync(string name, int? id = null)
        {
              var query = """
                 SELECT * FROM Budget.MiscTypeMaster
                 WHERE MiscTypeCode = @Name AND IsDeleted = 0
                 """;

             var parameters = new DynamicParameters(new { Name = name });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }

            return await _dbConnection.QueryFirstOrDefaultAsync<BudgetManagement.Domain.Entities.MiscTypeMaster>(query, parameters);
        } 

            public async Task<BudgetManagement.Domain.Entities.MiscTypeMaster> GetByIdAsync(int id)
        {            
           const string query = @" SELECT Id,MiscTypeCode,Description,IsActive  FROM Budget.MiscTypeMaster          
             WHERE Id = @id AND IsDeleted = 0";                          
            return await _dbConnection.QueryFirstOrDefaultAsync<BudgetManagement.Domain.Entities.MiscTypeMaster>(query, new { id }) ?? new BudgetManagement.Domain.Entities.MiscTypeMaster();
        }
          public async Task<List<BudgetManagement.Domain.Entities.MiscTypeMaster>>  GetMiscTypeMaster(string searchPattern)
        {
            

            const string query = @"SELECT Id, MiscTypeCode   FROM Budget.MiscTypeMaster
                WHERE IsDeleted = 0 AND MiscTypeCode LIKE @SearchPattern ";
                
            
            var parameters = new 
              { 
                  SearchPattern = $"%{searchPattern ?? string.Empty}%", 
               
              };

            var misctypemaster = await _dbConnection.QueryAsync<BudgetManagement.Domain.Entities.MiscTypeMaster>(query, parameters);
            return misctypemaster.ToList();
        }
          public async Task<bool> AlreadyExistsAsync(string miscTypeCode, int? id = null)
        {   

              var query = "SELECT COUNT(1) FROM Budget.MiscTypeMaster WHERE MiscTypeCode = @miscTypeCode AND IsDeleted = 0";
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
            var query = "SELECT COUNT(1) FROM  Budget.MiscTypeMaster WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            // True when the record is NOT found (honest to the method name). Validators use
            // !NotFoundAsync(id) so an existing row passes and a missing row fails "not found".
            return count == 0;
        }

         public async Task<bool> SoftDeleteValidation(int Id)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Budget].[MiscMaster]
                    WHERE MiscTypeId = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { Id });
            return result == 1;
        }

        public async Task<bool> IsMiscTypeMasterLinkedAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Budget].[MiscMaster]
                    WHERE MiscTypeId = @id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return result == 1;
        }

    }
}