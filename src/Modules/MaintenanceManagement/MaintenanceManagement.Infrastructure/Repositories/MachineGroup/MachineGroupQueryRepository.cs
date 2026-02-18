#nullable disable
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.MachineGroup
{
    public class MachineGroupQueryRepository : IMachineGroupQueryRepository
    {
       
       private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public MachineGroupQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;

        }
       
       public async Task<MaintenanceManagement.Domain.Entities.MachineGroup>GetByIdAsync(int id)
        { 
            var UnitId = _ipAddressService.GetUnitId();           
            const string query = @"
                SELECT 
                    Id,  GroupName,DepartmentId,Manufacturer,UnitId, IsActive, IsDeleted,PowerSource,CreatedBy, CreatedDate, CreatedByName,CreatedIP
                FROM Maintenance.MachineGroup          
                WHERE Id = @id AND UnitId = @UnitId AND IsDeleted = 0  ";
                                
            return await _dbConnection.QueryFirstOrDefaultAsync<MaintenanceManagement.Domain.Entities.MachineGroup>(query, new { id , UnitId });
        }
        public async Task<bool> GetByMachineGroupCodeAsync(string groupName, int id)
        {
            var UnitId = _ipAddressService.GetUnitId();    
            var query = """
            SELECT COUNT(1) FROM Maintenance.MachineGroup
            WHERE GroupName = @GroupName AND UnitId = @UnitId AND IsDeleted = 0 AND Id <> @Id
            """;

            var result = await _dbConnection.ExecuteScalarAsync<int>(query, new { GroupName = groupName, Id = id , UnitId });

            return result > 0;
        }
   

        public async Task<bool> GetByMachineGroupnameAsync(string groupName)
        {
            var UnitId = _ipAddressService.GetUnitId();
            var query = """
            SELECT COUNT(1) FROM Maintenance.MachineGroup
            WHERE GroupName = @GroupName  AND UnitId = @UnitId  AND IsDeleted = 0  
            """;

            var result = await _dbConnection.ExecuteScalarAsync<int>(query, new { GroupName = groupName  , UnitId });

            return result > 0;
        }
         public async Task<bool> NotFoundAsync(int id)
        {
            var UnitId = _ipAddressService.GetUnitId();
             var query = "SELECT COUNT(1) FROM Maintenance.MachineGroup WHERE Id = @Id AND IsDeleted = 0 AND UnitId = @UnitId";
             
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id , UnitId });
                return count > 0;
        }   
        public async Task<(List<MaintenanceManagement.Domain.Entities.MachineGroup>, int)> GetAllMachineGroupsAsync(int PageNumber, int PageSize, string SearchTerm)
            {
               var UnitId = _ipAddressService.GetUnitId();
                var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM [Maintenance].[MachineGroup] M
                WHERE M.IsDeleted = 0 AND M.UnitId = @UnitId 
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.GroupName LIKE @Search)")}}; 

                SELECT M.Id, M.GroupName, M.Manufacturer,M.DepartmentId,M.UnitId,M.IsActive, M.IsDeleted,M.PowerSource, 
                    M.CreatedBy, M.CreatedDate, M.CreatedByName, M.CreatedIP, 
                    M.ModifiedBy, M.ModifiedDate, M.ModifiedByName, M.ModifiedIP
                FROM [Maintenance].[MachineGroup] M
                WHERE M.IsDeleted = 0 AND M.UnitId = @UnitId 
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.GroupName LIKE @Search)")}}
                ORDER BY M.Id DESC 
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
                """;

                var parameters = new
                {
                    Search = $"%{SearchTerm}%",
                    Offset = (PageNumber - 1) * PageSize,
                    PageSize,
                    UnitId
                };

                var result = await _dbConnection.QueryMultipleAsync(query, parameters);
                
                // Read the data for MachineGroup and convert to list
                var machineGroupList = (await result.ReadAsync<MaintenanceManagement.Domain.Entities.MachineGroup>()).ToList();
                
                // Read the total count
                int totalCount = await result.ReadFirstAsync<int>();

                return (machineGroupList, totalCount);
            }
             public async Task<List<MaintenanceManagement.Domain.Entities.MachineGroup>> GetMachineGroupAutoComplete(string searchPattern)
               {

                var UnitId = _ipAddressService.GetUnitId();
                   const string query = @"
                       SELECT Id, GroupName  
                       FROM Maintenance.MachineGroup
                       WHERE IsDeleted = 0 AND GroupName LIKE @SearchPattern AND UnitId = @UnitId  AND IsActive = 1";

                   var parameters = new
                   {
                       SearchPattern = $"%{searchPattern ?? string.Empty}%",
                       UnitId
                   };
               var machineGroups = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MachineGroup>(query, parameters);
                   return machineGroups.ToList();
               }  
            public async Task<bool> FKColumnExistValidation(int machineGroupId)
            {
                var sql = "SELECT COUNT(1) FROM Maintenance.MachineGroup WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";
                  var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = machineGroupId });
                  return count > 0;
            }
        public async Task<bool> IsMachineGroupLinkedAsync(int id)
        {
            const string query = @"
        SELECT CASE WHEN
            EXISTS (
                SELECT 1
                FROM [Maintenance].[Maintenance].[MachineMaster] mm
                WHERE mm.IsDeleted = 0 AND mm.MachineGroupId = @id
            )
            OR EXISTS (
                SELECT 1
                FROM [Maintenance].[Maintenance].[MachineGroupUser] mgu
                WHERE mgu.IsDeleted = 0 AND mgu.MachineGroupId = @id
            )
        THEN 1 ELSE 0 END;
    ";

            var exists = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return exists == 1;
        }

    }
}