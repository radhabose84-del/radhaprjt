#nullable disable
using System.Data;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Domain.Entities;
using Dapper;
using static MaintenanceManagement.Domain.Common.MiscEnumEntity;

namespace MaintenanceManagement.Infrastructure.Repositories.PreventiveSchedulers
{
    public class PreventiveSchedulerQueryRepository : IPreventiveSchedulerQuery
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
        public PreventiveSchedulerQueryRepository(IDbConnection dbConnection, IIPAddressService iPAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = iPAddressService;
        }
        public async Task<bool> AlreadyExistsAsync(int activityId, int machinegroupId, int? id = null)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            var query = @"
                    SELECT COUNT(1) FROM [Maintenance].[PreventiveSchedulerHeader] PSH
                 INNER JOIN [Maintenance].[PreventiveSchedulerActivity] PSA ON PSA.PreventiveSchedulerHeaderId = PSH.Id
                   WHERE PSA.ActivityId = @ActivityId AND PSH.MachineGroupId =@MachineGroupId   AND PSH.IsDeleted = 0 AND PSH.IsActive = 1 AND PSH.UnitId=@UnitId ";
            var parameters = new DynamicParameters(new { ActivityId = activityId, MachineGroupId = machinegroupId, UnitId });

            if (id is not null)
            {
                query += " AND PSH.Id != @Id";
                parameters.Add("Id", id);
            }
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
            return count > 0;
        }

#pragma warning disable CS1998


#pragma warning restore CS1998
        #pragma warning disable CS1998
        public async Task<(DateTime nextDate, DateTime reminderDate)> CalculateNextScheduleDate(DateTime startDate, int interval, string unit, int reminderDays)
        #pragma warning restore CS1998
        {
            DateTime nextDate = unit.ToLower() switch
            {
                "days" => startDate.AddDays(interval),
                "months" => startDate.AddMonths(interval),
                "years" => startDate.AddYears(interval),
                _ => throw new ArgumentException("Unsupported frequency unit.")
            };
            DateTime reminderDate = nextDate.AddDays(-reminderDays);

            return (nextDate, reminderDate);
        }

        public async Task<(IEnumerable<dynamic> PreventiveSchedulerList, int)> GetAllPreventiveSchedulerAsync(int PageNumber, int PageSize, string SearchTerm, List<int> departmentIds)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM [Maintenance].[PreventiveSchedulerHeader] PS
                INNER JOIN [Maintenance].[MachineGroup] MG ON PS.MachineGroupId = MG.Id
                INNER JOIN [Maintenance].[MiscMaster] MC ON MC.Id = PS.MaintenanceCategoryId
                INNER JOIN [Maintenance].[MiscMaster] Schedule ON Schedule.Id = PS.ScheduleId
                INNER JOIN [Maintenance].[MiscMaster] FrequencyType ON FrequencyType.Id = PS.FrequencyTypeId
                INNER JOIN [Maintenance].[MiscMaster] FrequencyUnit ON FrequencyUnit.Id = PS.FrequencyUnitId
                WHERE PS.IsDeleted = 0 AND PS.UnitId=@UnitId AND PS.DepartmentId IN @DepartmentIds
                {(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (PS.PreventiveSchedulerName LIKE @Search OR MG.GroupName LIKE @Search OR MC.Code LIKE @Search OR Schedule.Code LIKE @Search OR FrequencyType.Code LIKE @Search OR FrequencyUnit.Code LIKE @Search)")};

                SELECT  
                    PS.Id,
                    PS.DepartmentId,
                    PS.FrequencyInterval,
                    PS.PreventiveSchedulerName,
                    PS.EffectiveDate,
                    PS.GraceDays,
                    PS.ReminderWorkOrderDays,
                    PS.ReminderMaterialReqDays,
                    PS.IsDownTimeRequired,
                    PS.DownTimeEstimateHrs,
                    PS.IsActive,
                    PS.CreatedBy,
                   Cast(PS.CreatedDate as varchar)  AS CreatedDate,
                    PS.CreatedByName,
                    PS.[CreatedIP],
                    PS.[ModifiedBy],
                    PS.[ModifiedDate],
                    PS.[ModifiedByName],
                    PS.[ModifiedIP],
                    MG.Id AS MachineGroupId,
                    MG.GroupName AS MachineGroup,
                    MG.DepartmentId AS ProductionDepartmentId,
                    MC.Id AS CategoryId,
                    MC.Code AS CategoryName ,
                    Schedule.Id AS ScheduleId,
                    Schedule.Code AS Schedule,
                    FrequencyType.Id AS FrequencyTypeId,
                    FrequencyType.Code AS FrequencyType,
                    FrequencyUnit.Id AS FrequencyUnitId,
                    FrequencyUnit.Code AS FrequencyUnit
                FROM [Maintenance].[PreventiveSchedulerHeader] PS
                INNER JOIN [Maintenance].[MachineGroup] MG ON PS.MachineGroupId = MG.Id
                INNER JOIN [Maintenance].[MiscMaster] MC ON MC.Id = PS.MaintenanceCategoryId
                INNER JOIN [Maintenance].[MiscMaster] Schedule ON Schedule.Id = PS.ScheduleId
                INNER JOIN [Maintenance].[MiscMaster] FrequencyType ON FrequencyType.Id = PS.FrequencyTypeId
                INNER JOIN [Maintenance].[MiscMaster] FrequencyUnit ON FrequencyUnit.Id = PS.FrequencyUnitId
                WHERE PS.IsDeleted = 0 AND PS.UnitId=@UnitId AND PS.DepartmentId IN @DepartmentIds AND PS.IsActive=1
                {(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (PS.PreventiveSchedulerName LIKE @Search OR MG.GroupName LIKE @Search OR MC.Code LIKE @Search OR Schedule.Code LIKE @Search OR FrequencyType.Code LIKE @Search OR FrequencyUnit.Code LIKE @Search)")}
                ORDER BY PS.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize = PageSize,
                UnitId,
                departmentIds
            };

            using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var preventiveSchedulers = await multi.ReadAsync<dynamic>();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (preventiveSchedulers, totalCount);
        }

        public async Task<PreventiveSchedulerHeader> GetByIdAsync(int id)
        {
            const string query = @"
            SELECT  
                    PS.Id,
                    PS.PreventiveSchedulerName,
                    PS.MachineGroupId,
                    PS.DepartmentId,
                    PS.MaintenanceCategoryId,
                    PS.ScheduleId,
                    PS.FrequencyTypeId,
                    PS.FrequencyInterval,
                    PS.FrequencyUnitId,
                    PS.EffectiveDate,
                    PS.GraceDays,
                    PS.ReminderWorkOrderDays,
                    PS.ReminderMaterialReqDays,
                    PS.IsDownTimeRequired,
                    PS.DownTimeEstimateHrs,
                    PS.IsActive,
                    PSA.Id,
                    PSA.PreventiveSchedulerHeaderId,
                    PSA.ActivityId,
                    PSI.Id,
                    PSI.PreventiveSchedulerHeaderId,
                    PSI.OldItemId,
                    PSI.RequiredQty,
                    PSI.OldCategoryDescription,
                    PSI.OldGroupName,
                    PSI.OldItemName
                FROM [Maintenance].[PreventiveSchedulerHeader] PS
                INNER JOIN [Maintenance].[PreventiveSchedulerActivity] PSA ON PSA.PreventiveSchedulerHeaderId = PS.Id
                LEFT JOIN [Maintenance].[PreventiveSchedulerItems] PSI ON PSI.PreventiveSchedulerHeaderId = PS.Id
                WHERE PS.IsDeleted = 0 AND PS.Id = @Id";
            var PreventiveSchedulerDictionary = new Dictionary<int, PreventiveSchedulerHeader>();

            var PreventiveSchedulerResponse = await _dbConnection.QueryAsync<PreventiveSchedulerHeader, PreventiveSchedulerActivity, PreventiveSchedulerItems, PreventiveSchedulerHeader>(
                query,
                (preventiveScheduler, preventiveSchedulerActivity, preventiveSchedulerItems) =>
                {
                    if (!PreventiveSchedulerDictionary.TryGetValue(preventiveScheduler.Id, out var existingPreventiveScheduler))
                    {
                        existingPreventiveScheduler = preventiveScheduler;
                        existingPreventiveScheduler.PreventiveSchedulerActivities = new List<PreventiveSchedulerActivity>();
                        existingPreventiveScheduler.PreventiveSchedulerItems = new List<PreventiveSchedulerItems>();
                        PreventiveSchedulerDictionary[preventiveScheduler.Id] = existingPreventiveScheduler;
                    }

                    if (!existingPreventiveScheduler.PreventiveSchedulerActivities!
                        .Any(a => a.Id == preventiveSchedulerActivity.Id))
                    {
                        existingPreventiveScheduler.PreventiveSchedulerActivities.Add(preventiveSchedulerActivity);
                    }

                    if (preventiveSchedulerItems != null &&
               preventiveSchedulerItems.OldItemId != null &&
               !existingPreventiveScheduler.PreventiveSchedulerItems!
                   .Any(i => i.Id == preventiveSchedulerItems.Id))
                    {
                        existingPreventiveScheduler.PreventiveSchedulerItems.Add(preventiveSchedulerItems);
                    }


                    return existingPreventiveScheduler;
                },
                new { id },
                splitOn: "Id,Id"
                );

            return PreventiveSchedulerResponse.FirstOrDefault()!;
        }



        public async Task<List<PreventiveSchedulerDetail>> GetPreventiveSchedulerDetail(int PreventiveSchedulerId)
        {
            var query = $@"
                  SELECT  
                  distinct  PSD.*
                    
                FROM [Maintenance].[PreventiveSchedulerDetail] PSD
				LEFT JOIN [Maintenance].[WorkOrder] WO ON WO.PreventiveScheduleId=PSD.Id
                LEFT Join [Maintenance].[MiscMaster] WOStatus ON WOStatus.Id=WO.StatusId 
				WHERE PSD.PreventiveSchedulerHeaderId =@PreventiveSchedulerId AND PSD.IsDeleted = 0 AND PSD.IsActive = 1
				AND (
           WO.Id IS NULL
           OR WOStatus.Code IN @Status
           )
            ";

            var statuses = new List<string> { StatusOpen.Code };
            var parameters = new
            {
                PreventiveSchedulerId = PreventiveSchedulerId,
                Status = statuses
            };
            var result = await _dbConnection.QueryAsync<PreventiveSchedulerDetail>(query, parameters);

            return result.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            var query = "SELECT COUNT(1) FROM [Maintenance].[PreventiveSchedulerHeader] WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundDetailAsync(int id)
        {
            var query = "SELECT COUNT(1) FROM [Maintenance].[PreventiveSchedulerDetail] WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public Task<bool> SoftDeleteValidation(int Id)
        {
            throw new NotImplementedException();
        }
        public async Task<DateTimeOffset?> GetLastMaintenanceDateAsync(int machineId, int PreventiveDetailId, string miscType, string misccode)
        {
            const string query = @"
                 SELECT MAX(WS.EndTime) AS EndTime   FROM [Maintenance].[WorkOrder] W
            INNER JOIN [Maintenance].[WorkOrderSchedule] WS ON W.Id=WS.WorkOrderId
            where W.PreventiveScheduleId=@PreventiveSchedulerId
            ";

            var parameters = new
            {
                PreventiveSchedulerId = PreventiveDetailId
            };

            var result = await _dbConnection.QueryFirstOrDefaultAsync<DateTimeOffset?>(query, parameters);
            return result;
        }

        public async Task<PreventiveSchedulerDetail> GetPreventiveSchedulerDetailById(int Id)
        {
            var query = $@"
                   SELECT
                     
                       *
                       
                   FROM [Maintenance].[PreventiveSchedulerDetail] WHERE Id =@Id AND IsDeleted = 0
               ";

            var parameters = new
            {
                Id = Id
            };
            var result = await _dbConnection.QueryFirstOrDefaultAsync<PreventiveSchedulerDetail>(query, parameters);

            return result;
        }
        public async Task<bool> UpdateValidation(int id)
        {
            var query = @"SELECT COUNT(1) FROM [Maintenance].[PreventiveSchedulerHeader] PSH
                 INNER JOIN [Maintenance].[PreventiveSchedulerDetail] PSD ON PSD.PreventiveSchedulerHeaderId=PSH.Id
                INNER JOIN [Maintenance].[WorkOrder] W ON W.PreventiveScheduleId =PSD.Id
                  WHERE PSH.Id = @Id AND PSH.IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }
        public async Task<IEnumerable<dynamic>> GetAbstractSchedulerByDate(int DepartmentId)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            var statusCodes = new[] { StatusOpen.Code, GetStatusId.Status };
            var query = $@"
                            SELECT
                        COUNT(PSD.MachineId) AS TotalScheduleCount,
                        CONVERT(varchar(10), CONVERT(date, PSD.ActualWorkOrderDate), 23) AS ScheduleDate, -- yyyy-mm-dd
                        PS.DepartmentId
                    FROM Maintenance.PreventiveSchedulerHeader AS PS
                    JOIN Maintenance.PreventiveSchedulerDetail  AS PSD ON PSD.PreventiveSchedulerHeaderId = PS.Id
                    LEFT JOIN Maintenance.WorkOrder            AS WO  ON WO.PreventiveScheduleId = PSD.Id
                    LEFT JOIN Maintenance.MiscMaster           AS MISC ON MISC.Id = WO.StatusId
                    WHERE
                        PS.IsDeleted = 0
                        AND PSD.IsDeleted = 0
                        AND PS.UnitId = @UnitId
                        AND PS.DepartmentId = @DepartmentId
                        AND (
                            -- Active: include fresh (no WO) OR WO in allowed statuses
                            (PS.IsActive = 1 AND PSD.IsActive = 1
                                AND (WO.Id IS NULL OR MISC.Code IN @StatusCodes)
                            )
                            OR
                            -- Inactive: include ONLY if WO exists AND status is allowed
                            ((PS.IsActive = 0 OR PSD.IsActive = 0)
                                AND WO.Id IS NOT NULL
                                AND MISC.Code IN @StatusCodes
                            )
                        )
                    GROUP BY
                        CONVERT(date, PSD.ActualWorkOrderDate),
                        PS.DepartmentId
                    ORDER BY
    CONVERT(date, PSD.ActualWorkOrderDate) ASC;
                        ";
            using var multi = await _dbConnection.QueryMultipleAsync(query, new { UnitId, StatusCodes = statusCodes, DepartmentId });
            var preventiveSchedulers = await multi.ReadAsync<dynamic>();

            return preventiveSchedulers;
        }
        public async Task<IEnumerable<dynamic>> GetDetailSchedulerByDate(DateOnly schedulerDate, int DepartmentId)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            var statusCodes = new[] { StatusOpen.Code, GetStatusId.Status, WorkOrderHold.Code };
            var query = $@"
                            SELECT
                        PS.Id AS HeaderId,
                        PSD.Id AS DetailId,
                        WO.Id AS WorkOrderId,
                        PS.PreventiveSchedulerName,
                        PS.MachineGroupId,
                        MG.GroupName,
                        PSD.MachineId,
                        M.MachineCode,
                        M.MachineName,
                        PS.DepartmentId
                    FROM Maintenance.PreventiveSchedulerHeader AS PS
                    JOIN Maintenance.PreventiveSchedulerDetail  AS PSD ON PSD.PreventiveSchedulerHeaderId = PS.Id
                    JOIN Maintenance.MachineMaster             AS M   ON M.Id = PSD.MachineId
                    JOIN Maintenance.MachineGroup              AS MG  ON MG.Id = PS.MachineGroupId
                    LEFT JOIN Maintenance.WorkOrder            AS WO  ON WO.PreventiveScheduleId = PSD.Id
                    LEFT JOIN Maintenance.MiscMaster           AS MISC ON MISC.Id = WO.StatusId
                    WHERE
                        PS.IsDeleted = 0
                        AND PSD.IsDeleted = 0
                        AND PS.UnitId = @UnitId
                        AND PS.DepartmentId = @DepartmentId
                        AND CONVERT(date, PSD.ActualWorkOrderDate) = CONVERT(date, @ActualWorkOrderDate)
                        AND (
                            -- Active schedules: fresh OR allowed WO status
                            (PS.IsActive = 1 AND PSD.IsActive = 1
                                AND (WO.Id IS NULL OR MISC.Code IN @StatusCodes)
                            )
                            OR
                            -- Inactive schedules: must have WO and allowed status
                            ((PS.IsActive = 0 OR PSD.IsActive = 0)
                                AND WO.Id IS NOT NULL
                                AND MISC.Code IN @StatusCodes
                            )
                        )
                    ORDER BY
                     PS.Id ASC, PSD.Id ASC;

                        ";
            var parameters = new
            {
                ActualWorkOrderDate = schedulerDate,
                UnitId,
                StatusCodes = statusCodes,
                DepartmentId
            };
            using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var preventiveSchedulers = await multi.ReadAsync<dynamic>();

            return preventiveSchedulers;
        }
        public async Task<PreventiveSchedulerHeader> GetWorkOrderScheduleDetailById(int Id)
        {
            var query = $@"
                    SELECT  
						 PSH.Id,
						PSH.CompanyId,
                       PSH.UnitId,
                       PSH.MaintenanceCategoryId,
                       PSD.Id,
                       PSA.ActivityId,
                       PSI.OldItemId
					   FROM [Maintenance].[PreventiveSchedulerDetail] PSD
				   INNER JOIN [Maintenance].[PreventiveSchedulerHeader] PSH ON PSD.PreventiveSchedulerHeaderId=PSH.Id
				   INNER JOIN [Maintenance].[PreventiveSchedulerActivity] PSA ON PSA.PreventiveSchedulerHeaderId=PSD.PreventiveSchedulerHeaderId
				   LEFT JOIN [Maintenance].[PreventiveSchedulerItems] PSI ON PSI.PreventiveSchedulerHeaderId=PSD.PreventiveSchedulerHeaderId
				   WHERE PSD.Id =@Id AND PSD.IsDeleted = 0  AND PSH.IsDeleted = 0
               ";

            var PreventiveSchedulerDictionary = new Dictionary<int, PreventiveSchedulerHeader>();

            var PreventiveSchedulerResponse = await _dbConnection.QueryAsync<PreventiveSchedulerHeader, PreventiveSchedulerDetail, PreventiveSchedulerActivity, PreventiveSchedulerItems, PreventiveSchedulerHeader>(
            query,
            (preventiveScheduler, preventiveSchedulerDetail, preventiveSchedulerActivity, preventiveSchedulerItems) =>
            {
                if (!PreventiveSchedulerDictionary.TryGetValue(preventiveScheduler.Id, out var existingPreventiveScheduler))
                {
                    existingPreventiveScheduler = preventiveScheduler;
                    existingPreventiveScheduler.PreventiveSchedulerDetails = new List<PreventiveSchedulerDetail>();
                    existingPreventiveScheduler.PreventiveSchedulerActivities = new List<PreventiveSchedulerActivity>();
                    existingPreventiveScheduler.PreventiveSchedulerItems = new List<PreventiveSchedulerItems>();
                    PreventiveSchedulerDictionary[preventiveScheduler.Id] = existingPreventiveScheduler;
                }

                existingPreventiveScheduler.PreventiveSchedulerDetails!.Add(preventiveSchedulerDetail);

                existingPreventiveScheduler.PreventiveSchedulerActivities!.Add(preventiveSchedulerActivity);
                if (preventiveSchedulerItems != null && preventiveSchedulerItems.OldItemId != null)
                {
                    existingPreventiveScheduler.PreventiveSchedulerItems!.Add(preventiveSchedulerItems);
                }


                return existingPreventiveScheduler;
            },
            new { Id },
            splitOn: "Id,ActivityId,OldItemId"
            );

            return PreventiveSchedulerResponse.FirstOrDefault()!;
        }
        public async Task<bool> MachingroupValidation(int id)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            var query = @"SELECT COUNT(1) FROM [Maintenance].[MachineGroup] MG
                 INNER JOIN [Maintenance].[MachineMaster] MM ON MM.MachineGroupId=MG.Id
                  WHERE MG.Id = @Id AND MG.IsDeleted = 0 AND MG.IsActive = 1 AND MM.IsDeleted = 0 AND MM.IsActive = 1 AND MM.Unitid=@UnitId";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id, UnitId });
            return count > 0;
        }
        public async Task<bool> ExistWorkOrderBySchedulerDetailId(int id)
        {

            var query = @"SELECT COUNT(1) FROM [Maintenance].[WorkOrder]
                  WHERE PreventiveScheduleId = @Id ";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<bool> ExistPreventivescheduleItem(int id)
        {
            var query = @"SELECT COUNT(*) FROM [Maintenance].[PreventiveSchedulerDetail] PSD
				   INNER JOIN [Maintenance].[PreventiveSchedulerItems] PSI ON PSI.PreventiveSchedulerHeaderId=PSD.PreventiveSchedulerHeaderId
				   WHERE PSD.Id =@Id  AND PSD.IsDeleted = 0 ";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<PreventiveSchedulerHeader> GetWorkOrderScheduleDetailWithoutItemidById(int Id)
        {
            var query = $@"
                    SELECT  
						 PSH.Id,
						PSH.CompanyId,
                       PSH.UnitId,
                       PSH.MaintenanceCategoryId,
                       PSD.Id,
                       PSA.ActivityId
					   FROM [Maintenance].[PreventiveSchedulerDetail] PSD
				   INNER JOIN [Maintenance].[PreventiveSchedulerHeader] PSH ON PSD.PreventiveSchedulerHeaderId=PSH.Id
				   INNER JOIN [Maintenance].[PreventiveSchedulerActivity] PSA ON PSA.PreventiveSchedulerHeaderId=PSD.PreventiveSchedulerHeaderId
				   WHERE PSD.Id =@Id AND PSD.IsDeleted = 0  AND PSH.IsDeleted = 0
               ";

            var PreventiveSchedulerDictionary = new Dictionary<int, PreventiveSchedulerHeader>();

            var PreventiveSchedulerResponse = await _dbConnection.QueryAsync<PreventiveSchedulerHeader, PreventiveSchedulerDetail, PreventiveSchedulerActivity, PreventiveSchedulerHeader>(
            query,
            (preventiveScheduler, preventiveSchedulerDetail, preventiveSchedulerActivity) =>
            {
                if (!PreventiveSchedulerDictionary.TryGetValue(preventiveScheduler.Id, out var existingPreventiveScheduler))
                {
                    existingPreventiveScheduler = preventiveScheduler;
                    existingPreventiveScheduler.PreventiveSchedulerDetails = new List<PreventiveSchedulerDetail>();
                    existingPreventiveScheduler.PreventiveSchedulerActivities = new List<PreventiveSchedulerActivity>();
                    PreventiveSchedulerDictionary[preventiveScheduler.Id] = existingPreventiveScheduler;
                }

                existingPreventiveScheduler.PreventiveSchedulerDetails!.Add(preventiveSchedulerDetail);

                existingPreventiveScheduler.PreventiveSchedulerActivities!.Add(preventiveSchedulerActivity);


                return existingPreventiveScheduler;
            },
            new { Id },
            splitOn: "Id,ActivityId"
            );

            return PreventiveSchedulerResponse.FirstOrDefault()!;
        }
        public async Task<MaintenanceManagement.Domain.Entities.MachineGroup> GetMachineGroupIdByName(string MachineGroupName)
        {
            const string query = @"
                       SELECT Id, GroupName  
                       FROM Maintenance.MachineGroup
                       WHERE IsDeleted = 0 AND GroupName = @SearchPattern";

            var parameters = new
            {
                SearchPattern = MachineGroupName
            };
            var machineGroups = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MachineGroup>(query, parameters);
            return machineGroups.FirstOrDefault();
        }

        public async Task<MaintenanceManagement.Domain.Entities.MachineMaster> GetMachineIdByCode(string MachineCode)
        {
            const string query = @"
                       SELECT Id, MachineCode  
                       FROM [Maintenance].[MachineMaster]
                       WHERE IsDeleted = 0 AND MachineCode = @SearchPattern";

            var parameters = new
            {
                SearchPattern = MachineCode
            };
            var machines = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MachineMaster>(query, parameters);
            return machines.FirstOrDefault();
        }
        public async Task<PreventiveSchedulerDetail> GetPreventiveSchedulerDetailByName(string PreventiveSchedulerName, string MachineId)
        {
            var query = $@"
                SELECT  
                    PSD.Id,
                    PSD.PreventiveSchedulerHeaderId,
                    PSD.MachineId,
                    PSD.WorkOrderCreationStartDate,
                    PSD.ActualWorkOrderDate,
                    PSD.RescheduleReason,
                    PSD.MaterialReqStartDays,
                    PSD.HangfireJobId
                    
                FROM [Maintenance].[PreventiveSchedulerDetail] PSD
        INNER JOIN [Maintenance].[PreventiveSchedulerHeader] PSH ON PSD.PreventiveSchedulerHeaderId=PSH.Id
        WHERE PreventiveSchedulerName =@PreventiveSchedulerName AND PSD.MachineId=@MachineId AND PSD.IsDeleted = 0
            ";

            var parameters = new
            {
                PreventiveSchedulerName,
                MachineId
            };
            var result = await _dbConnection.QueryAsync<PreventiveSchedulerDetail>(query, parameters);

            return result.FirstOrDefault();
        }

        public async Task<MaintenanceManagement.Domain.Entities.ActivityMaster> GetActivityIdByName(string ActivityName)
        {
            const string query = @"
                       SELECT Id, ActivityName  
                       FROM Maintenance.ActivityMaster
                       WHERE IsDeleted = 0 AND IsActive = 1 AND ActivityName = @SearchPattern";

            var parameters = new
            {
                SearchPattern = ActivityName
            };
            var machineGroups = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.ActivityMaster>(query, parameters);
            return machineGroups.FirstOrDefault();
        }

        public async Task<PreventiveSchedulerHeader> GetDetailSchedulerByPreventiveScheduleId(int Id)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            var statusCodes = new[] { StatusOpen.Code, GetStatusId.Status, WorkOrderHold.Code };
            // var query = $@"
            //                 SELECT  
            //                     WO.WorkOrderDocNo,PS.PreventiveSchedulerName,MG.GroupName,M.MachineCode,M.MachineName,PS.DepartmentId
            //                 FROM [Maintenance].[PreventiveSchedulerHeader] PS
            //                 INNER JOIN [Maintenance].[PreventiveSchedulerDetail] PSD ON PSD.PreventiveSchedulerHeaderId = PS.Id
            //                 INNER JOIN [Maintenance].[MachineMaster] M ON M.Id =PSD.MachineId
            //                 INNER JOIN [Maintenance].[MachineGroup] MG ON MG.Id = PS.MachineGroupId
            //                 LEFT JOIN Maintenance.WorkOrder WO ON WO.PreventiveScheduleId=PSD.Id
            // 				LEFT JOIN Maintenance.MiscMaster MISC ON MISC.Id=WO.StatusId
            //                 WHERE PS.IsDeleted = 0 AND PSD.IsDeleted =0 AND PS.Id=@PreventiveScheduleId AND PS.UnitId=@UnitId 
            //                 AND (MISC.Code IN @StatusCodes OR WO.Id IS NULL) AND PSD.IsActive=1 
            //                 ORDER BY PS.Id ASC
            //             ";
            // var parameters = new
            // {
            //     PreventiveScheduleId = Id,
            //     UnitId,
            //     StatusCodes = statusCodes
            // };
            // using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            // var preventiveSchedulers = await multi.ReadAsync<dynamic>();

            // return preventiveSchedulers;

            const string query = @"
            SELECT  
                    PS.Id,
                    PS.PreventiveSchedulerName,
                    PS.DepartmentId,
                    PS.FrequencyInterval,
                    PS.GraceDays,
                    PS.ReminderWorkOrderDays,
                    PS.ReminderMaterialReqDays,
                    PS.IsDownTimeRequired,
                    PS.DownTimeEstimateHrs,
                    PS.IsActive,
                    PSD.Id,
                    PSD.MachineId,
                    PSD.WorkOrderCreationStartDate,
                    PSD.ActualWorkOrderDate,
                    PSD.LastMaintenanceActivityDate,
                    PSD.FrequencyInterval,
                    PSD.IsActive,
                    PSA.Id,
                    PSA.PreventiveSchedulerHeaderId,
                    PSA.ActivityId,
                    PSI.Id,
                    PSI.PreventiveSchedulerHeaderId,
                    PSI.OldItemId,
                    PSI.RequiredQty,
                    PSI.OldCategoryDescription,
                    PSI.OldGroupName,
                    PSI.OldItemName,
                    M.MachineName,
                    M.MachineCode,
                    MG.GroupName,
                    AM.ActivityName
                FROM [Maintenance].[PreventiveSchedulerHeader] PS
                INNER JOIN [Maintenance].[PreventiveSchedulerActivity] PSA ON PSA.PreventiveSchedulerHeaderId = PS.Id
                INNER JOIN [Maintenance].[ActivityMaster] AM ON AM.Id =PSA.ActivityId
                LEFT JOIN [Maintenance].[PreventiveSchedulerItems] PSI ON PSI.PreventiveSchedulerHeaderId = PS.Id
                INNER JOIN [Maintenance].[PreventiveSchedulerDetail] PSD ON PSD.PreventiveSchedulerHeaderId = PS.Id
                INNER JOIN [Maintenance].[MachineMaster] M ON M.Id =PSD.MachineId
                INNER JOIN [Maintenance].[MachineGroup] MG ON MG.Id = PS.MachineGroupId
                LEFT JOIN Maintenance.WorkOrder WO ON WO.PreventiveScheduleId=PSD.Id
                LEFT JOIN Maintenance.MiscMaster MISC ON MISC.Id=WO.StatusId
                WHERE PS.IsDeleted = 0 AND PSD.IsDeleted =0 AND PS.Id=@PreventiveScheduleId AND PS.UnitId=@UnitId 
                            AND (MISC.Code IN @StatusCodes OR WO.Id IS NULL) AND PSD.IsActive=1
                ORDER BY M.MachineName ASC";
            var PreventiveSchedulerDictionary = new Dictionary<int, PreventiveSchedulerHeader>();

            var PreventiveSchedulerResponse = await _dbConnection.QueryAsync<PreventiveSchedulerHeader, PreventiveSchedulerDetail, PreventiveSchedulerActivity, PreventiveSchedulerItems, MaintenanceManagement.Domain.Entities.MachineMaster, MaintenanceManagement.Domain.Entities.MachineGroup, MaintenanceManagement.Domain.Entities.ActivityMaster, PreventiveSchedulerHeader>(
                query,
                (preventiveScheduler, preventiveScheduleDetail, preventiveSchedulerActivity, preventiveSchedulerItems, machine, group, activity) =>
                {
                    if (!PreventiveSchedulerDictionary.TryGetValue(preventiveScheduler.Id, out var existingPreventiveScheduler))
                    {
                        existingPreventiveScheduler = preventiveScheduler;
                        existingPreventiveScheduler.PreventiveSchedulerDetails = new List<PreventiveSchedulerDetail>();
                        existingPreventiveScheduler.PreventiveSchedulerActivities = new List<PreventiveSchedulerActivity>();
                        existingPreventiveScheduler.PreventiveSchedulerItems = new List<PreventiveSchedulerItems>();
                        existingPreventiveScheduler.MachineGroup = group;
                        PreventiveSchedulerDictionary[preventiveScheduler.Id] = existingPreventiveScheduler;
                    }
                    if (!existingPreventiveScheduler.PreventiveSchedulerDetails!
                        .Any(a => a.Id == preventiveScheduleDetail.Id))
                    {
                        preventiveScheduleDetail.Machine = machine;
                        existingPreventiveScheduler.PreventiveSchedulerDetails.Add(preventiveScheduleDetail);
                    }

                    if (!existingPreventiveScheduler.PreventiveSchedulerActivities!
                        .Any(a => a.Id == preventiveSchedulerActivity.Id))
                    {
                        preventiveSchedulerActivity.Activity = activity;
                        existingPreventiveScheduler.PreventiveSchedulerActivities.Add(preventiveSchedulerActivity);
                    }

                    if (preventiveSchedulerItems != null &&
               preventiveSchedulerItems.OldItemId != null &&
               !existingPreventiveScheduler.PreventiveSchedulerItems!
                   .Any(i => i.Id == preventiveSchedulerItems.Id))
                    {
                        existingPreventiveScheduler.PreventiveSchedulerItems.Add(preventiveSchedulerItems);
                    }


                    return existingPreventiveScheduler;
                },
                new
                {
                    PreventiveScheduleId = Id,
                    UnitId,
                    StatusCodes = statusCodes
                },
                splitOn: "Id,Id,Id,MachineName,GroupName,ActivityName"
                );

            return PreventiveSchedulerResponse.FirstOrDefault()!;
        }
        public async Task<List<MaintenanceManagement.Domain.Entities.MachineMaster>> GetUnMappedMachineIdByCode(int Id)
        {
            // var statusCodes = new[] { StatusOpen.Code, GetStatusId.Status, WorkOrderHold.Code };

            const string query = @"
                                   WITH LatestMachineStatus AS (
                SELECT *,
                       ROW_NUMBER() OVER (PARTITION BY MachineId ORDER BY Id DESC) AS rn
                FROM [Maintenance].[PreventiveSchedulerDetail]
                WHERE PreventiveSchedulerHeaderId = @Id
            )
            SELECT L.MachineId, L.IsActive
            INTO #notActiveMachine
            FROM LatestMachineStatus L
            LEFT JOIN Maintenance.WorkOrder WO ON WO.PreventiveScheduleId = L.Id
			LEFT JOIN [Maintenance].[MiscMaster] MM ON MM.Id = WO.StatusId
            WHERE rn = 1 AND (L.IsActive = 0 or L.IsDeleted=1 or MM.Code=@Status);

            -- Step 2: Get machines not in PreventiveSchedulerDetail (unmapped)
            SELECT M.Id, M.MachineCode, M.MachineName  
            FROM [Maintenance].[MachineMaster] M
            INNER JOIN [Maintenance].[MachineGroup] MG ON M.MachineGroupId = MG.Id
            INNER JOIN [Maintenance].[PreventiveSchedulerHeader] PSH ON PSH.Id = @Id AND MG.Id = PSH.MachineGroupId
            LEFT JOIN [Maintenance].[PreventiveSchedulerDetail] PSD ON M.Id = PSD.MachineId AND PSD.PreventiveSchedulerHeaderId = PSH.Id
            WHERE M.IsDeleted = 0 AND M.IsActive=1 AND PSD.Id IS NULL

            UNION

            -- Step 3: Get machines with latest status as inactive
            SELECT M.Id, M.MachineCode, M.MachineName  
            FROM [Maintenance].[MachineMaster] M
            INNER JOIN #notActiveMachine A ON M.Id = A.MachineId;

            -- Step 4: Cleanup
            DROP TABLE #notActiveMachine;";

            var parameters = new
            {
                Id,
                Status = MaintenanceStatusCancelled.Code,
                //  DoneStatus = MaintenanceStatusUpdate.Code
            };
            var machines = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.MachineMaster>(query, parameters);
            return machines.ToList();
        }
        public async Task<bool> OneTimeSchedulerValidate(int preventiveSchedulerId, CancellationToken ct)
        {
            const string sql = $@"
                   SELECT COUNT(*)
        FROM Maintenance.PreventiveSchedulerDetail AS PSD
        WHERE PSD.PreventiveSchedulerHeaderId = @preventiveSchedulerId
          AND PSD.IsDeleted = 0
          AND EXISTS (
              SELECT 1
              FROM Maintenance.WorkOrder AS WO
              WHERE WO.PreventiveScheduleId = PSD.Id
          );
            ";


            var cmd = new CommandDefinition(
         commandText: sql,
         parameters: new { preventiveSchedulerId },
         cancellationToken: ct
     );

            var count = await _dbConnection.ExecuteScalarAsync<int>(cmd);
            return count > 0;
        }
        public async Task<PreventiveSchedulerHeader> OnetimeFrequencyValidation(int id, CancellationToken ct)
        {

            const string sql = @"
                   SELECT
                       PS.Id,
                       PS.FrequencyTypeId,
                       MISC.Id,
                       MISC.Code
                   FROM Maintenance.PreventiveSchedulerHeader AS PS
                   INNER JOIN Maintenance.MiscMaster AS MISC ON MISC.Id = PS.FrequencyTypeId
                   WHERE PS.IsDeleted = 0 AND PS.Id = @PreventiveScheduleId;
               ";

            var cmd = new CommandDefinition(
                commandText: sql,
                parameters: new { PreventiveScheduleId = id },
                cancellationToken: ct
            );

            var rows = await _dbConnection.QueryAsync<
                PreventiveSchedulerHeader,
                MaintenanceManagement.Domain.Entities.MiscMaster,
                PreventiveSchedulerHeader
            >(
                cmd,
                map: (header, misc) =>
                {
                    header.MiscFrequencyType = misc;
                    return header;
                },
                splitOn: "Id"
            );

            return rows.FirstOrDefault();
        }
         public async Task<List<int>> WorkOrderNotGeneratedScheduler(int PreventiveSchedulerId)
        {
            var query = $@"
                 SELECT PSD.Id
                FROM Maintenance.PreventiveSchedulerDetail AS PSD
                WHERE PSD.PreventiveSchedulerHeaderId = @PreventiveSchedulerId
                  AND PSD.IsDeleted = 0 AND PSD.IsActive = 1
                  AND NOT EXISTS (
                        SELECT 1
                        FROM Maintenance.WorkOrder AS WO
                        WHERE WO.PreventiveScheduleId = PSD.Id
                      );

            ";

            
            var parameters = new
            {
                PreventiveSchedulerId
            };
            var result = await _dbConnection.QueryAsync<int>(query, parameters);

            return result.ToList();
        }
        public async Task<DateOnly?> GetClosedDateBySchedulerDetailId(int preventiveScheduleDetailId)
        {
            const string sql = @"
        SELECT MAX(CONVERT(date, WS.EndTime))
        FROM Maintenance.WorkOrder WO
        JOIN Maintenance.MiscMaster MS ON MS.Id = WO.StatusId
        JOIN Maintenance.WorkOrderSchedule WS ON WS.WorkOrderId = WO.Id
        WHERE WO.PreventiveScheduleId = @preventiveScheduleDetailId
          AND MS.Code = @ClosedCode;
        ";

            var dt = await _dbConnection.ExecuteScalarAsync<DateTime?>(
                sql,
                new { preventiveScheduleDetailId, ClosedCode = MaintenanceStatusUpdate.Code } // "Closed"
            );

            return dt.HasValue ? DateOnly.FromDateTime(dt.Value) : null;
        }
    }
}