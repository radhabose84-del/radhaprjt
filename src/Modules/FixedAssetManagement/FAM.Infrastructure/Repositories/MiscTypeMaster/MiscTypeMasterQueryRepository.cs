#nullable disable
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Interfaces.IMiscTypeMaster;
using Dapper;
using FAM.Domain.Entities;

namespace FAM.Infrastructure.Repositories.MiscTypeMaster
{
    public class MiscTypeMasterQueryRepository  : IMiscTypeMasterQueryRepository
    {
          private readonly IDbConnection _dbConnection;


    public MiscTypeMasterQueryRepository(IDbConnection dbConnection)
            {
            _dbConnection = dbConnection;
            }



          public async Task<(List<FAM.Domain.Entities.MiscTypeMaster>,int)> GetAllMiscTypeMasterAsync(int PageNumber, int PageSize, string SearchTerm)
        {
                 var query = $$"""
                    DECLARE @TotalCount INT;
                    SELECT @TotalCount = COUNT(*) 
                    FROM [FixedAsset].[MiscTypeMaster] M
                    WHERE M.IsDeleted = 0
                    {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.MiscTypeCode LIKE @Search)")}}; 

                    SELECT M.Id, M.MiscTypeCode, M.Description, M.IsActive, M.IsDeleted, M.CreatedBy, 
                        M.CreatedDate, M.CreatedByName, M.CreatedIP, M.ModifiedBy, M.ModifiedDate, 
                        M.ModifiedByName, M.ModifiedIP  
                    FROM FixedAsset.MiscTypeMaster M
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
             var misctypemaster = (await misctype.ReadAsync<FAM.Domain.Entities.MiscTypeMaster>()).ToList();
             int totalCount = (await misctype.ReadFirstAsync<int>());
            return (misctypemaster, totalCount);
        }
          public async Task<FAM.Domain.Entities.MiscTypeMaster> GetByMiscTypeMasterCodeAsync(string name, int? id = null)
        {
              var query = """
                 SELECT * FROM FixedAsset.MiscTypeMaster
                 WHERE MiscTypeCode = @Name AND IsDeleted = 0
                 """;

             var parameters = new DynamicParameters(new { Name = name });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }

            return await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.MiscTypeMaster>(query, parameters);
        } 

            public async Task<FAM.Domain.Entities.MiscTypeMaster> GetByIdAsync(int id)
        {            
           const string query = @" SELECT Id,MiscTypeCode,Description,IsActive  FROM FixedAsset.MiscTypeMaster          
             WHERE Id = @id AND IsDeleted = 0";                          
            return await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.MiscTypeMaster>(query, new { id });
        }
          public async Task<List<FAM.Domain.Entities.MiscTypeMaster>>  GetMiscTypeMaster(string searchPattern)
        {
            

            const string query = @"SELECT Id, MiscTypeCode   FROM FixedAsset.MiscTypeMaster
                WHERE IsDeleted = 0 AND MiscTypeCode LIKE @SearchPattern ";
                
            
            var parameters = new 
              { 
                  SearchPattern = $"%{searchPattern ?? string.Empty}%", 
                //   CompanyId = CompanyId 
              };

            var misctypemaster = await _dbConnection.QueryAsync<FAM.Domain.Entities.MiscTypeMaster>(query, parameters);
            return misctypemaster.ToList();
        }
        
        
    }
}