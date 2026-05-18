using System.Data;
using Contracts.Interfaces;
using Dapper;
using MaintenanceManagement.Application.Common.Interfaces.IServiceHistory;
using MaintenanceManagement.Application.ServiceHistory.Dto;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.Common;

namespace MaintenanceManagement.Infrastructure.Repositories.ServiceHistory
{
    public class ServiceHistoryQueryRepository : BaseQueryRepository, IServiceHistoryQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public ServiceHistoryQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
            : base(ipAddressService)
        {
            _dbConnection = dbConnection;
        }

        // Read-only union of the three immutable maintenance sources. There is no
        // dedicated history table — the feed "auto-updates" because it is derived
        // from records that are written by the normal maintenance flow.
        private const string UnionSql = @"
            SELECT 'WorkOrder' AS EventType, WO.Id AS SourceId,
                   COALESCE(psd.MachineId, mr.MachineId) AS MachineId,
                   CAST(WO.WorkOrderDocNo AS varchar(50))      AS DocNo,
                   CAST(st.Description AS nvarchar(200))        AS ActionOrStatus,
                   CAST(WO.Remarks AS nvarchar(1000))           AS Description,
                   CAST(COALESCE(WO.DowntimeEnd, WO.ModifiedDate, WO.CreatedDate) AS datetimeoffset) AS PerformedOn,
                   CAST(WO.TotalSpentHours AS decimal(9,2))     AS TotalSpentHours,
                   CAST(WO.TotalManPower AS int)                AS TotalManPower,
                   CAST(NULL AS varchar(50))                    AS Source,
                   CAST(NULL AS bit)                            AS IsSuccess,
                   WO.CreatedBy,
                   CAST(WO.CreatedByName AS varchar(50))        AS CreatedByName
            FROM Maintenance.WorkOrder WO
            INNER JOIN Maintenance.MiscMaster st ON st.Id = WO.StatusId
            LEFT JOIN Maintenance.PreventiveSchedulerDetail psd ON psd.Id = WO.PreventiveScheduleId
            LEFT JOIN Maintenance.MaintenanceRequest mr ON mr.Id = WO.RequestId
            WHERE WO.CompanyId = @CompanyId AND WO.UnitId = @UnitId AND st.Code = @ClosedCode

            UNION ALL

            SELECT 'PreventiveScheduleLog', PSL.Id,
                   psd.MachineId,
                   CAST(NULL AS varchar(50)),
                   CAST(PSL.ActionType AS nvarchar(200)),
                   CAST(PSL.Remarks AS nvarchar(1000)),
                   CAST(PSL.CreatedDate AS datetimeoffset),
                   CAST(NULL AS decimal(9,2)),
                   CAST(NULL AS int),
                   CAST(PSL.Source AS varchar(50)),
                   CAST(PSL.IsSuccess AS bit),
                   PSL.CreatedBy,
                   CAST(PSL.CreatedByName AS varchar(50))
            FROM Maintenance.PreventiveScheduleLog PSL
            INNER JOIN Maintenance.PreventiveSchedulerDetail psd
                ON psd.Id = PSL.PreventiveScheduleDetailId AND psd.IsDeleted = 0
            WHERE PSL.PreventiveScheduleDetailId IS NOT NULL

            UNION ALL

            SELECT 'MaintenanceRequest', MR.Id,
                   MR.MachineId,
                   CAST(NULL AS varchar(50)),
                   CAST(rs.Description AS nvarchar(200)),
                   CAST(MR.Remarks AS nvarchar(1000)),
                   CAST(COALESCE(MR.ModifiedDate, MR.CreatedDate) AS datetimeoffset),
                   CAST(NULL AS decimal(9,2)),
                   CAST(NULL AS int),
                   CAST(NULL AS varchar(50)),
                   CAST(NULL AS bit),
                   MR.CreatedBy,
                   CAST(MR.CreatedByName AS varchar(50))
            FROM Maintenance.MaintenanceRequest MR
            LEFT JOIN Maintenance.MiscMaster rs ON rs.Id = MR.RequestStatusId
            WHERE MR.CompanyId = @CompanyId AND MR.UnitId = @UnitId AND MR.IsDeleted = 0";

        private const string FilterSql = @"
            FROM History H
            INNER JOIN Maintenance.MachineMaster mm ON mm.Id = H.MachineId AND mm.IsDeleted = 0
            WHERE (@MachineId IS NULL OR H.MachineId = @MachineId)
              AND (@AssetId   IS NULL OR mm.AssetId  = @AssetId)
              AND (@FromDate  IS NULL OR H.PerformedOn >= @FromDate)
              AND (@ToDate    IS NULL OR H.PerformedOn <= @ToDate)";

        public async Task<(List<ServiceHistoryDto> Items, int TotalCount)> GetServiceHistoryAsync(
            int? machineId,
            int? assetId,
            DateTimeOffset? fromDate,
            DateTimeOffset? toDate,
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", CompanyId);
            parameters.Add("@UnitId", UnitId);
            parameters.Add("@ClosedCode", MiscEnumEntity.MaintenanceStatusUpdate.Code);
            parameters.Add("@MachineId", machineId);
            parameters.Add("@AssetId", assetId);
            parameters.Add("@FromDate", fromDate);
            parameters.Add("@ToDate", toDate);
            parameters.Add("@Offset", (pageNumber - 1) * pageSize);
            parameters.Add("@PageSize", pageSize);

            var pageSql = $@"
                ;WITH History AS ({UnionSql})
                SELECT H.EventType, H.SourceId, H.MachineId,
                       mm.MachineCode, mm.MachineName, mm.AssetId,
                       H.DocNo, H.ActionOrStatus, H.Description, H.PerformedOn,
                       H.TotalSpentHours, H.TotalManPower, H.Source, H.IsSuccess,
                       H.CreatedBy, H.CreatedByName
                {FilterSql}
                ORDER BY H.PerformedOn DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                ;WITH History AS ({UnionSql})
                SELECT COUNT(1)
                {FilterSql};";

            using var multi = await _dbConnection.QueryMultipleAsync(pageSql, parameters);

            var items = (await multi.ReadAsync<ServiceHistoryDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (items, totalCount);
        }
    }
}
