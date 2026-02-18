#nullable disable
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.MiscMaster
{
    public class MiscMasterQueryRepository : IMiscMasterQueryRepository
    {
    private readonly IDbConnection _dbConnection;

     public MiscMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;

        }

        public async Task<(List<MaintenanceManagement.Domain.Entities.MiscMaster>,int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string SearchTerm)
        {
                var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM [Maintenance].[MiscMaster] M
                WHERE M.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.Code LIKE @Search)")}}; 

                SELECT M.Id, M.MiscTypeId, M.Code, M.Description, M.SortOrder, M.IsActive, M.IsDeleted, 
                    M.CreatedBy, M.CreatedDate, M.CreatedByName, M.CreatedIP, M.ModifiedBy, M.ModifiedDate, 
                    M.ModifiedByName, M.ModifiedIP
                FROM Maintenance.MiscMaster M
                WHERE M.IsDeleted = 0 
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.Code LIKE @Search)")}}
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

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            
            // Read the data for MiscMaster and convert to list
            var miscMasterList = (await result.ReadAsync<MaintenanceManagement.Domain.Entities.MiscMaster>()).ToList();
            
            // Read the total count
            int totalCount = await result.ReadFirstAsync<int>();

            return (miscMasterList, totalCount);
                
            }

            
            public async Task<MaintenanceManagement.Domain.Entities.MiscMaster> GetByIdAsync(int id)
        {            
           const string query = @" SELECT Id,MiscTypeId,Code,Description,SortOrder,IsActive  FROM Maintenance.MiscMaster          
             WHERE Id = @id AND IsDeleted = 0 ";                          
            return await _dbConnection.QueryFirstOrDefaultAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query, new { id });
        } 


        public async Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>>  GetMiscMaster(string searchPattern,string miscTypeCode)
        {
            

            const string query = @"SELECT M.Id,M.Code ,M.Description  FROM Maintenance.MiscMaster M
            INNER JOIN [Maintenance].[MiscTypeMaster] MT ON MT.Id = M.MiscTypeId
                WHERE M.IsDeleted = 0 AND MT.IsDeleted = 0 AND M.IsActive = 1  AND MT.MiscTypeCode= @MiscTypeCode AND M.Code LIKE @SearchPattern  ";
                
            
            var parameters = new 
              { 
                  SearchPattern = $"%{searchPattern ?? string.Empty}%",
                  MiscTypeCode = miscTypeCode 
             
              };

            var miscmaster = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query, parameters);
            return miscmaster.ToList();
        }

        public async Task<MaintenanceManagement.Domain.Entities.MiscMaster> GetByMiscMasterCodeAsync(string name, int? id = null)
        {
              var query = """
                 SELECT * FROM Maintenance.MiscMaster
                 WHERE Code = @Name AND IsDeleted = 0 
                 """;

             var parameters = new DynamicParameters(new { Name = name });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }

            return await _dbConnection.QueryFirstOrDefaultAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query, parameters);
        } 
        
        public async Task<MaintenanceManagement.Domain.Entities.MiscMaster> GetByWFMiscMasterCodeAsync(string name, int? id = null)
        {
              var query = """
                 SELECT * FROM Appdata.MiscMaster
                 WHERE Code = @Name AND IsDeleted = 0 
                 """;

             var parameters = new DynamicParameters(new { Name = name });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }

            return await _dbConnection.QueryFirstOrDefaultAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query, parameters);
        } 

               public async Task<int> GetMaxSortOrderAsync()
        {
            var query = "SELECT ISNULL(MAX(SortOrder), 0) FROM Maintenance.MiscMaster WHERE IsDeleted = 0 ";
            return await _dbConnection.QueryFirstOrDefaultAsync<int>(query);
        }
        
        public async Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null)
        {
            var query = @"SELECT COUNT(1) 
                        FROM Maintenance.MiscMaster 
                        WHERE Code = @Code 
                            AND MiscTypeId = @MiscTypeId  
                            AND IsDeleted = 0 
                            ";

            var parameters = new DynamicParameters(new 
            { 
                Code = code, 
                MiscTypeId = miscTypeId 
            });

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
             var query = "SELECT COUNT(1) FROM Maintenance.MiscMaster WHERE Id = @Id AND IsDeleted = 0";
             
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
                return count > 0;
        } 

           public async Task<bool> FKColumnValidation(int MiscMasterId)
        {
            var query = "SELECT COUNT(1) FROM Maintenance.MiscMaster WHERE Id = @Id AND IsDeleted = 0   ";
             
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = MiscMasterId });
                return count > 0;
        }
          public async Task<MaintenanceManagement.Domain.Entities.MiscMaster>  GetMiscMasterByName(string miscTypeCode,string miscTypeName)
        {
            

            const string query = @"SELECT M.Id,M.Code ,M.Description  FROM Maintenance.MiscMaster AS M
                                INNER JOIN Maintenance.MiscTypeMaster AS MT 
                                ON MT.Id = M.MiscTypeId
                                WHERE M.IsDeleted = 0 AND MT.IsDeleted = 0 AND M.IsActive = 1 AND MT.MiscTypeCode= @MiscTypeCode AND M.Code=@MiscTypeName  ";
                
            
            var parameters = new 
              { 
                  MiscTypeName = miscTypeName,
                  MiscTypeCode = miscTypeCode 
             
              };

            var miscmaster = await _dbConnection.QueryFirstOrDefaultAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query, parameters);
            return miscmaster;
        }      



                  
    }   

    
} 