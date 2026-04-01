#nullable disable
using System.Data;
using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMaster;
using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetCheckListByActivityId;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.ActivityCheckListMaster
{
    public class ActivityCheckListMasterQueryRepository : IActivityCheckListMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;


        public ActivityCheckListMasterQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {

            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;

        }
        public async Task<(List<GetAllActivityCheckListMasterDto>, int)> GetAllActivityCheckListMasterAsync(int PageNumber, int PageSize, string SearchTerm)
        {
           var UnitId = _ipAddressService.GetUnitId() ?? 0;

            var query = $$"""
                    DECLARE @TotalCount INT;
                    SELECT @TotalCount = COUNT(DISTINCT aclm.Id)
                    FROM Maintenance.Maintenance.ActivityCheckListMaster aclm
                    INNER JOIN Maintenance.ActivityMaster am 
                        ON aclm.ActivityID = am.Id
                    WHERE aclm.IsDeleted = 0 AND aclm.UnitId = @UnitId   AND am.UnitId = @UnitId  
                    {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (am.ActivityName LIKE @Search OR aclm.ActivityChecklist LIKE @Search)")}};

                    SELECT 
                        aclm.Id AS ChecklistId,
                        aclm.ActivityID,
                        am.ActivityName,
                        aclm.ActivityChecklist ,
                        aclm.UnitId,
                        am.DepartmentId,
                        aclm.IsActive,
                        aclm.IsDeleted,
                        aclm.CreatedBy,
                        aclm.CreatedDate,
                        aclm.CreatedByName,
                        aclm.CreatedIP,
                        aclm.ModifiedBy,
                        aclm.ModifiedDate,
                        aclm.ModifiedByName,
                        aclm.ModifiedIP
                    FROM Maintenance.ActivityCheckListMaster aclm
                    INNER JOIN Maintenance.ActivityMaster am 
                        ON aclm.ActivityID = am.Id
                    WHERE aclm.IsDeleted = 0   AND aclm.UnitId = @UnitId   AND am.UnitId = @UnitId  
                    {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (am.ActivityName LIKE @Search OR aclm.ActivityChecklist LIKE @Search)")}}
                    ORDER BY aclm.Id DESC 
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
            var checkListMasters = (await result.ReadAsync<GetAllActivityCheckListMasterDto>()).ToList();
            int totalCount = await result.ReadFirstAsync<int>();

            return (checkListMasters, totalCount);
        }

        public async Task<GetAllActivityCheckListMasterDto> GetByIdAsync(int id)
        {

            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            const string query = @"
                SELECT 
                    aclm.Id AS ChecklistId,
                    aclm.ActivityID,
                    am.ActivityName,
                    aclm.ActivityChecklist  ,
                    am.DepartmentId,
                    aclm.UnitId,
                    aclm.IsActive,
                    aclm.IsDeleted,
                    aclm.CreatedBy,
                    aclm.CreatedDate,
                    aclm.CreatedByName,
                    aclm.CreatedIP,
                    aclm.ModifiedBy,
                    aclm.ModifiedDate,
                    aclm.ModifiedByName,
                    aclm.ModifiedIP
                FROM Maintenance.ActivityCheckListMaster aclm
                INNER JOIN Maintenance.ActivityMaster am 
                    ON aclm.ActivityID = am.Id
                WHERE aclm.Id = @id AND aclm.IsDeleted = 0 AND aclm.UnitId = @UnitId AND am.UnitId = @UnitId ";

            return await _dbConnection.QueryFirstOrDefaultAsync<GetAllActivityCheckListMasterDto>(query, new { id, UnitId });
        }


        public async Task<bool> GetByActivityCheckListAsync(string activityCheckList, int activityId)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            var query = """
            SELECT COUNT(1) FROM Maintenance.ActivityCheckListMaster
            WHERE ActivityCheckList = @activityCheckList AND ActivityID = @activityId  AND IsDeleted = 0  AND UnitId = @UnitId  
            """;

            var result = await _dbConnection.ExecuteScalarAsync<int>(query, new { ActivityCheckList = activityCheckList, ActivityID = activityId  , UnitId});

            return result > 0;
        }

        public async Task<bool> AlreadyExistsCheckListAsync(string activityCheckList, int activityId, int? id = null)
        {
           var UnitId = _ipAddressService.GetUnitId() ?? 0;
            var query = "SELECT COUNT(1) FROM Maintenance.ActivityCheckListMaster WHERE ActivityCheckList = @activityCheckList AND ActivityID = @activityId AND IsDeleted = 0 AND UnitId = @UnitId AND IsActive = 1"; 
            var parameters = new DynamicParameters(new { ActivityCheckList = activityCheckList, ActivityID = activityId , UnitId});

            if (id is not null)
            {
                query += " AND Id != @Id";
                parameters.Add("Id", id);
            }
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
            return count > 0;
        }
        public async Task<List<GetActivityCheckListByActivityIdDto>> GetCheckListByActivityIdsAsync(List<int> ids, int? workOrderId = null)
        {

            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            if (ids == null || !ids.Any())
            {
                return new List<GetActivityCheckListByActivityIdDto>();
            }

            const string query = @"
                    SELECT 
                        aclm.Id AS ChecklistId,
                        aclm.ActivityID,
                        am.ActivityName,
                        aclm.ActivityChecklist,
                        am.DepartmentId,
                        aclm.UnitId,
                        aclm.IsActive,
                        aclm.IsDeleted,
                        aclm.CreatedBy,
                        aclm.CreatedDate,
                        aclm.CreatedByName,
                        aclm.CreatedIP,
                        aclm.ModifiedBy,
                        aclm.ModifiedDate,
                        aclm.ModifiedByName,
                        aclm.ModifiedIP
                    FROM Maintenance.ActivityCheckListMaster aclm
                    INNER JOIN Maintenance.ActivityMaster am ON aclm.ActivityID = am.Id
                    LEFT JOIN Maintenance.WorkOrderCheckList WOC 
                        ON WOC.CheckListId = aclm.Id 
                        AND (@WorkOrderId IS NULL OR WOC.WorkOrderId = @WorkOrderId)
                    WHERE aclm.ActivityID IN @Ids
                        AND aclm.IsDeleted = 0  AND aclm.UnitId = @UnitId AND am.UnitId = @UnitId
                    GROUP BY 
                        aclm.Id, aclm.ActivityID, am.ActivityName, aclm.ActivityChecklist, aclm.IsActive, 
                        aclm.IsDeleted, aclm.CreatedBy, aclm.CreatedDate, aclm.CreatedByName, aclm.CreatedIP,
                        aclm.ModifiedBy, aclm.ModifiedDate, aclm.ModifiedByName, aclm.ModifiedIP,
                        am.DepartmentId, aclm.UnitId ";

            var parameters = new
            {
                Ids = ids,
                WorkOrderId = workOrderId,
                UnitId
            };

            var result = await _dbConnection.QueryAsync<GetActivityCheckListByActivityIdDto>(query, parameters);
            return result.ToList();
        }

        public async Task<bool> IsActivityCheckListMasterLinkedAsync(int id)
        {
            const string query = @"
        SELECT TOP 1 1
        FROM [Maintenance].[WorkOrderCheckList]
        WHERE CheckListId = @id;
        ";

            var exists = await _dbConnection.QueryFirstOrDefaultAsync<int?>(query, new { id });
            return exists.HasValue;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string query = @"
                SELECT COUNT(1) FROM [Maintenance].[WorkOrderCheckList]
                WHERE CheckListId = @Id";
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

    }
}