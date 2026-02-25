#nullable disable
using System.Data;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.ShiftMaster
{
    public class ShiftMasterQueryRepository : IShiftMasterQuery
    {
        private readonly IDbConnection _dbConnection; 
        public ShiftMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<bool> AlreadyExistsAsync(string ShiftName, int? id = null)
        {   

              var query = "SELECT COUNT(1) FROM [Maintenance].[ShiftMaster] WHERE ShiftName = @ShiftName AND IsDeleted = 0";
                var parameters = new DynamicParameters(new { ShiftName = ShiftName });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
                return count > 0;
        }
         public async Task<bool> AlreadyExistsShiftCodeAsync(string ShiftCode, int? id = null)
        {   

              var query = "SELECT COUNT(1) FROM [Maintenance].[ShiftMaster] WHERE ShiftCode = @ShiftCode AND IsDeleted = 0";
                var parameters = new DynamicParameters(new { ShiftCode = ShiftCode });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
                return count > 0;
        }

        public async Task<(List<MaintenanceManagement.Domain.Entities.ShiftMaster>, int)> GetAllShiftMasterAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM [Maintenance].[ShiftMaster] 
              WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ShiftName LIKE @Search OR ShiftCode LIKE @Search)")}};

                SELECT 
                Id, 
                ShiftCode,
                ShiftName,
                EffectiveDate,
                IsActive,CreatedDate
            FROM [Maintenance].[ShiftMaster] 
            WHERE 
            IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ShiftName LIKE @Search OR ShiftCode LIKE @Search )")}}
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

            var shiftmaster = await _dbConnection.QueryMultipleAsync(query, parameters);
            var shiftMasterlist = (await shiftmaster.ReadAsync<MaintenanceManagement.Domain.Entities.ShiftMaster>()).ToList();
            int totalCount = (await shiftmaster.ReadFirstAsync<int>());

            return (shiftMasterlist, totalCount);
        }

        public async Task<MaintenanceManagement.Domain.Entities.ShiftMaster> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM [Maintenance].[ShiftMaster]  WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<MaintenanceManagement.Domain.Entities.ShiftMaster>(query, new { id });
        }

        public async Task<List<MaintenanceManagement.Domain.Entities.ShiftMaster>> GetShiftMaster(string searchPattern)
        {
             const string query = @"
                SELECT Id, ShiftCode,ShiftName 
                FROM [Maintenance].[ShiftMaster] 
                WHERE IsDeleted = 0 AND ShiftName LIKE @SearchPattern";
                
            var shiftMasters = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.ShiftMaster>(query, new { SearchPattern = $"%{searchPattern}%" });
            return shiftMasters.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
             var query = "SELECT COUNT(1) FROM [Maintenance].[ShiftMaster] WHERE Id = @Id AND IsDeleted = 0";
             
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
                return count > 0;
        }

        public async Task<bool> SoftDeleteValidation(int Id)
        {
             const string query = @"
                           SELECT 1 
                           FROM [Maintenance].[ShiftMasterDetails]
                           WHERE ShiftMasterId = @Id AND IsDeleted = 0;";
                    
                       using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });
                    
                       var shiftMasterDetailExists = await multi.ReadFirstOrDefaultAsync<int?>();
                    
                       return shiftMasterDetailExists.HasValue;
        }
        
        public async Task<bool> IsShiftMasterLinkedAsync(int id)
        {
            const string query = @"
        SELECT CASE WHEN
            EXISTS (
                SELECT 1
                FROM [Maintenance].[Maintenance].[MachineMaster] mm
                WHERE mm.IsDeleted = 0 AND mm.ShiftMasterId = @id
            )
            OR EXISTS (
                SELECT 1
                FROM [Maintenance].[Maintenance].[ShiftMasterDetails] smd
                WHERE smd.IsDeleted = 0 AND smd.ShiftMasterId = @id
            )
        THEN 1 ELSE 0 END;
    ";

            var exists = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return exists == 1;
        }
    }
}