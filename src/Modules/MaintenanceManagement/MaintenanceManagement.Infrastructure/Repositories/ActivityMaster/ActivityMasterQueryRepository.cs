#nullable disable
using System.Data;
using System.Text.Json;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetActivityByMachinGroupId;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetAllActivityMaster;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetMachineGroupById;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById;
using MaintenanceManagement.Domain.Common;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.ActivityMaster
{
    public class ActivityMasterQueryRepository : IActivityMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public ActivityMasterQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;

        }


        public async Task<(List<GetAllActivityMasterDto>, int)> GetAllActivityMasterAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            var query = $$"""
                 DECLARE @TotalCount INT;
                 
                      SELECT @TotalCount = COUNT(*)
                    FROM [Maintenance].[ActivityMaster] A                               
                    INNER JOIN [Maintenance].[MiscMaster] C ON A.ActivityType = C.Id 
                    WHERE A.IsDeleted = 0 AND A.UnitId = @UnitId
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (A.ActivityName LIKE @Search OR A.Description LIKE @Search)")}}; 

                  SELECT  A.Id, A.ActivityName, A.Description, A.DepartmentId,   A.UnitId    ,             
                    A.EstimatedDuration, A.ActivityType, C.Code AS ActivityTypeDescription, 
                    A.IsActive, A.IsDeleted, 
                    A.CreatedBy, A.CreatedDate, A.CreatedByName, A.CreatedIP, 
                    A.ModifiedBy, A.ModifiedDate, A.ModifiedByName, A.ModifiedIP
                FROM [Maintenance].[ActivityMaster] A                                    
                INNER JOIN [Maintenance].[MiscMaster] C ON A.ActivityType = C.Id
                WHERE A.IsDeleted = 0 AND A.UnitId = @UnitId
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (A.ActivityName LIKE @Search OR A.Description LIKE @Search)")}}
                ORDER BY A.Id DESC 
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;
            var parameters = new
            {

                Search = string.IsNullOrWhiteSpace(SearchTerm) ? null : $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize = PageSize,
                UnitId
            };
            
            
            using var grid = await _dbConnection.QueryMultipleAsync(query, parameters);
            var items = (await grid.ReadAsync<GetAllActivityMasterDto>()).ToList();
            var totalCount = await grid.ReadSingleAsync<int>();

            return (items, totalCount);

            // var assetTransfers = await _dbConnection.QueryMultipleAsync(query, parameters);
            // var assetTransferList = (await assetTransfers.ReadAsync<GetAllActivityMasterDto>()).ToList();
            // int totalCount = await assetTransfers.ReadFirstAsync<int>();


            // return (assetTransferList, totalCount);


        }
        public async Task<GetActivityMasterByIdDto> GetByIdAsync(int activityMasterId)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            const string query = @"
                   
                    SELECT A.Id, A.ActivityName, A.Description, A.DepartmentId, A.UnitId,
                        B.DeptName AS Department, A.EstimatedDuration, A.ActivityType, 
                        C.Code AS ActivityTypeDescription, A.IsActive, A.IsDeleted
                    FROM [Maintenance].[ActivityMaster] A
                    INNER JOIN [BannariERP].[AppData].[Department] B ON A.DepartmentId = B.Id
                    INNER JOIN [Maintenance].[MiscMaster] C ON A.ActivityType = C.Id
                    WHERE A.Id = @ActivityMasterId AND A.IsDeleted = 0 AND A.UnitId = @UnitId
                    FOR JSON PATH, INCLUDE_NULL_VALUES;
             -- Fetch Related Machine Groups
                    SELECT  D.MachineGroupId, F.GroupName AS MachineGroupName
                    FROM [Maintenance].[ActivityMachineGroup] D
                    INNER JOIN [Maintenance].[MachineGroup] F ON D.MachineGroupId = F.Id                       
                    WHERE D.ActivityMasterId = @ActivityMasterId
                    FOR JSON PATH, INCLUDE_NULL_VALUES;
                ";
            using var multiQuery = await _dbConnection.QueryMultipleAsync(query, new { activityMasterId , UnitId });
            string activityJson = await multiQuery.ReadFirstOrDefaultAsync<string>() ?? "[]";
            string machineGroupsJson = await multiQuery.ReadFirstOrDefaultAsync<string>() ?? "[]";
            if (string.IsNullOrWhiteSpace(activityJson))
            {
                return null; // Return null if no activity found
            }
            var activity = JsonSerializer.Deserialize<List<GetActivityMasterByIdDto>>(activityJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })?.FirstOrDefault();
            var machineGroups = JsonSerializer.Deserialize<List<GetAllMachineGroupDto>>(machineGroupsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (activity != null)
            {
                activity.GetAllMachineGroupDto = machineGroups ?? new List<GetAllMachineGroupDto>();
            }
            return activity;
        }

        public async Task<List<GetMachineGroupNameByIdDto>> GetMachineGroupById(int activityId)
        {
            const string query = @"
                SELECT A.ActivityMasterId AS ActivityId, A.MachineGroupId, B.GroupName AS MachineGroupName
                FROM [Maintenance].[ActivityMachineGroup] A
                INNER JOIN [Maintenance].[MachineGroup] B ON A.MachineGroupId = B.Id
                WHERE A.ActivityMasterId = @ActivityMasterId
            ";

            var machineGroups = await _dbConnection.QueryAsync<GetMachineGroupNameByIdDto>(query, new { ActivityMasterId = activityId });

            return machineGroups.ToList();

        }
         public async Task<List<MaintenanceManagement.Domain.Entities.ActivityMaster>> GetActivityMasterAutoComplete(     string searchPattern ,string machineCode = null)
        {
             var unitId = _ipAddressService.GetUnitId() ?? 0;
                var code = string.IsNullOrWhiteSpace(machineCode) ? null : machineCode.Trim();

                const string sql = @"
                    SELECT DISTINCT a.Id, a.ActivityName
                    FROM Maintenance.ActivityMaster a
                    LEFT JOIN Maintenance.ActivityMachineGroup b ON b.ActivityMasterId = a.Id
                    LEFT JOIN Maintenance.MachineGroup      c ON c.Id = b.MachineGroupId
                    LEFT JOIN Maintenance.MachineMaster     d ON d.MachineGroupId = c.Id
                    WHERE a.IsDeleted = 0
                    AND a.IsActive  = 1
                    AND a.UnitId    = @UnitId
                    AND a.ActivityName LIKE @SearchPattern
                    AND (@MachineCode IS NULL OR d.MachineCode = @MachineCode)
                    ORDER BY a.ActivityName;";

                var parameters = new
                {
                    UnitId = unitId,
                    SearchPattern = $"%{(searchPattern ?? string.Empty).Trim()}%",
                    MachineCode = (object)code ?? DBNull.Value
                };

                var rows = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.ActivityMaster>(sql, parameters);
                return rows.ToList();
        }
        // public async Task<List<MaintenanceManagement.Domain.Entities.ActivityMaster>> GetActivityMasterAutoComplete(string searchPattern)
        // {

        //     var UnitId = _ipAddressService.GetUnitId() ?? 0;
        //     const string query = @"
        //                SELECT Id, ActivityName  
        //                FROM Maintenance.ActivityMaster
        //                WHERE IsDeleted = 0 AND IsActive = 1 AND ActivityName LIKE @SearchPattern AND UnitId = @UnitId";

        //     var parameters = new
        //     {
        //         SearchPattern = $"%{searchPattern ?? string.Empty}%",
        //         UnitId
        //     };
        //     var machineGroups = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.ActivityMaster>(query, parameters);
        //     return machineGroups.ToList();
        // }

        // public async Task<bool> GetByActivityNameAsync(string activityname , int activityId)
        // {
        //     var UnitId = _ipAddressService.GetUnitId() ?? 0;
        //     var query = """
        //             SELECT COUNT(1) FROM Maintenance.ActivityMaster
        //             WHERE ActivityName = @activityname AND IsDeleted = 0 AND IsActive = 1 AND UnitId = @UnitId AND Id != @activityId
        //             """;

        //     var result = await _dbConnection.ExecuteScalarAsync<int>(query, new { ActivityName = activityname  , UnitId , activityId });

        //     return result > 0;
        // }
        public async Task<bool> GetByActivityNameAsync(string activityname, int? activityId = null)
            {
                var unitId = _ipAddressService.GetUnitId() ?? 0;

                var baseQuery = """
                    SELECT COUNT(1)
                    FROM Maintenance.ActivityMaster
                    WHERE ActivityName = @ActivityName
                    AND IsDeleted = 0
                    AND IsActive = 1
                    AND UnitId = @UnitId
                """;

                var fullQuery = activityId.HasValue
                    ? baseQuery + " AND Id != @ActivityId"
                    : baseQuery;

                var result = await _dbConnection.ExecuteScalarAsync<int>(fullQuery, new
                {
                    ActivityName = activityname,
                    UnitId = unitId,
                    ActivityId = activityId
                });

                return result > 0;
            }

        public async Task<bool> FKColumnExistValidation(int activityId)
        {
            var sql = "SELECT COUNT(1) FROM Maintenance.ActivityMaster WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = activityId });
            return count > 0;
        }

        public async Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetActivityTypeAsync()
        {
            const string query = @"
                SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder            
                FROM Maintenance.MiscMaster M
                INNER JOIN Maintenance.MiscTypeMaster T on T.ID=M.MiscTypeId
                WHERE (MiscTypeCode = @MiscTypeCode) 
                AND  M.IsDeleted=0 and M.IsActive=1
                ORDER BY SortOrder DESC";
            var parameters = new { MiscTypeCode = MiscEnumEntity.GetActivityType.MiscCode };
            var result = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MiscMaster>(query, parameters);
            return result.ToList();
        } 
        public async Task<List<GetActivityByMachineGroupDto>> GetActivityByMachinGroupId(int machineGroupById)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
                    const string query = @"
                SELECT a.Id , a.ActivityName
                FROM Maintenance.ActivityMaster a
                INNER JOIN Maintenance.ActivityMachineGroup b ON a.Id = b.ActivityMasterId
                INNER JOIN Maintenance.MachineGroup c ON b.MachineGroupId = c.Id
                WHERE c.Id = @MachineGroupId AND a.IsDeleted = 0 AND a.UnitId = @UnitId
            ";

            var activityList = await _dbConnection.QueryAsync<GetActivityByMachineGroupDto>(query, new { MachineGroupId = machineGroupById  , UnitId});
            return activityList.ToList();

        }

        public async Task<bool> IsActivityMasterLinkedAsync(int id)
        {
        const string query = @"
        SELECT TOP 1 1
        FROM [Maintenance].[Maintenance].[ActivityCheckListMaster]
        WHERE IsDeleted = 0 AND ActivityID = @id;
        ";

            var exists = await _dbConnection.QueryFirstOrDefaultAsync<int?>(query, new { id });
            return exists.HasValue;
        }
        
    }
}