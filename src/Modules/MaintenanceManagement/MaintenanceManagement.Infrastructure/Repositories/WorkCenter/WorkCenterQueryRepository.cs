using System.Data;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.WorkCenter
{
    public class WorkCenterQueryRepository : IWorkCenterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public WorkCenterQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<MaintenanceManagement.Domain.Entities.WorkCenter>, int)> GetAllWorkCenterGroupAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
             var UnitId = _ipAddressService.GetUnitId() ?? 0;
            var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM Maintenance.WorkCenter
              WHERE IsDeleted = 0 AND UnitId = @UnitId
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (WorkCenterName LIKE @Search OR WorkCenterCode LIKE @Search)")}};

                SELECT 
                Id, 
                WorkCenterCode,
                WorkCenterName,
                UnitId,
                DepartmentId,
                IsActive,CreatedDate
            FROM Maintenance.WorkCenter 
            WHERE 
            IsDeleted = 0 AND UnitId = @UnitId
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (WorkCenterName LIKE @Search OR WorkCenterCode LIKE @Search )")}}
                ORDER BY Id desc
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

            var workCenter = await _dbConnection.QueryMultipleAsync(query, parameters);
            var workCenterslist = (await workCenter.ReadAsync<MaintenanceManagement.Domain.Entities.WorkCenter>()).ToList();
            int totalCount = (await workCenter.ReadFirstAsync<int>());
            return (workCenterslist, totalCount);
        }

        public async Task<MaintenanceManagement.Domain.Entities.WorkCenter?> GetByIdAsync(int Id)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            const string query = @"
                    SELECT * 
                    FROM Maintenance.WorkCenter 
                    WHERE Id = @Id AND IsDeleted = 0 AND UnitId = @UnitId";

            var workCenter = await _dbConnection.QueryFirstOrDefaultAsync<MaintenanceManagement.Domain.Entities.WorkCenter>(query, new { Id, UnitId });
            return workCenter;
        }

        public async Task<List<MaintenanceManagement.Domain.Entities.WorkCenter>> GetWorkCenterGroups(string searchPattern)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            searchPattern = searchPattern ?? string.Empty; // Prevent null issues

            const string query = @"
             SELECT Id, WorkCenterName 
            FROM Maintenance.WorkCenter 
            WHERE IsDeleted = 0 AND UnitId = @UnitId And IsActive = 1
            AND WorkCenterName LIKE @SearchPattern";
            var parameters = new
            {
                SearchPattern = $"%{searchPattern}%",
                UnitId
            };

            var workCenters = await _dbConnection.QueryAsync<MaintenanceManagement.Domain.Entities.WorkCenter>(query, parameters);
            return workCenters.ToList();
        }

        public async Task<bool> SoftDeleteValidation(int Id)
        {
            const string query = @"
                           SELECT 1 
                           FROM [Maintenance].[MachineMaster]
                           WHERE WorkCenterId = @Id AND IsDeleted = 0;";

            using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });

            var WorkCentermasterDetailExists = await multi.ReadFirstOrDefaultAsync<int?>();

            return WorkCentermasterDetailExists.HasValue;
        }

        public async Task<bool> IsWorkCenterLinkedAsync(int id)
        {
            const string query = @"
        SELECT TOP 1 1
        FROM [Maintenance].[MachineMaster]
        WHERE IsDeleted = 0 AND IsActive = 1 AND WorkCenterId = @id;
      ";

            var exists = await _dbConnection.QueryFirstOrDefaultAsync<int?>(query, new { id });
            return exists.HasValue;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string query = @"
                SELECT COUNT(1) FROM Maintenance.WorkCenter WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }
    }
}