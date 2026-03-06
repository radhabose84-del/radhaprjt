using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundService.Application.Dto;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService.Infrastructure.Repositories.Notification.NotificationWhatsAppGroup
{
    public class NotificationWhatsAppGroupQueryRepository : INotificationWhatsAppGroupQuery
    {
        private readonly IDbConnection _db;
        private readonly IIPAddressService _ipAddressService;

        public NotificationWhatsAppGroupQueryRepository(
            [FromKeyedServices("Notification")] IDbConnection db,
            IIPAddressService ipAddressService)
        {
            _db = db;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<NotificationWhatsAppGroupDto> Items, int TotalCount)> GetAllAsync(
            NotificationWhatsAppGroupListFilterDto filter,
            CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId();
            var skip = (filter.PageNumber - 1) * filter.PageSize;

            const string sql = @"
SELECT
    nwg.Id,
    nwg.UnitId,
    UnitName = CAST('' AS NVARCHAR(200)),
    nwg.DepartmentId,
    DepartmentName = CAST('' AS NVARCHAR(200)),
    nwg.GroupName,
    ApiKey = ISNULL(nwg.ApiKey, ''),
    IsActive = CASE WHEN nwg.IsActive = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END,
    IsDeleted = CASE WHEN nwg.IsDeleted = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END
FROM [AppNotification].[NotificationWhatsAppGroup] nwg WITH (NOLOCK)
WHERE
    nwg.IsDeleted = 0
    AND nwg.UnitId = @UnitId
    AND (@DepartmentId IS NULL OR nwg.DepartmentId = @DepartmentId)
    AND (@SearchTerm IS NULL OR nwg.GroupName LIKE @SearchTerm)
ORDER BY nwg.Id DESC
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

SELECT COUNT(1)
FROM [AppNotification].[NotificationWhatsAppGroup] nwg WITH (NOLOCK)
WHERE
    nwg.IsDeleted = 0
    AND nwg.UnitId = @UnitId
    AND (@DepartmentId IS NULL OR nwg.DepartmentId = @DepartmentId)
    AND (@SearchTerm IS NULL OR nwg.GroupName LIKE @SearchTerm);
";

            var param = new
            {
                UnitId = unitId,
                DepartmentId = filter.DepartmentId,
                SearchTerm = string.IsNullOrWhiteSpace(filter.SearchTerm) ? null : $"%{filter.SearchTerm.Trim()}%",
                Skip = skip,
                Take = filter.PageSize
            };

            using var multi = await _db.QueryMultipleAsync(sql, param);

            var items = (await multi.ReadAsync<NotificationWhatsAppGroupDto>()).ToList();
            var total = await multi.ReadSingleAsync<int>();

            return (items, total);
        }

        public async Task<NotificationWhatsAppGroupDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId();

            const string sql = @"
SELECT TOP 1
    nwg.Id,
    nwg.UnitId,
    UnitName = CAST('' AS NVARCHAR(200)),
    nwg.DepartmentId,
    DepartmentName = CAST('' AS NVARCHAR(200)),
    nwg.GroupName,
    ApiKey = ISNULL(nwg.ApiKey, ''),
    IsActive = CASE WHEN nwg.IsActive = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END,
    IsDeleted = CASE WHEN nwg.IsDeleted = 1 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END
FROM [AppNotification].[NotificationWhatsAppGroup] nwg WITH (NOLOCK)
WHERE
    nwg.Id = @Id
    AND nwg.UnitId = @UnitId
    AND nwg.IsDeleted = 0;";

            return await _db.QueryFirstOrDefaultAsync<NotificationWhatsAppGroupDto>(
                sql,
                new { Id = id, UnitId = unitId });
        }

        public async Task<List<NotificationWhatsAppGroupAutoCompleteDto>> GetAutoCompleteAsync(
            string? searchTerm,
            CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId();

            const string sql = @"
SELECT TOP 25
    nwg.Id,
    nwg.GroupName,
    nwg.DepartmentId,
    DepartmentName = CAST('' AS NVARCHAR(200))
FROM [AppNotification].[NotificationWhatsAppGroup] nwg WITH (NOLOCK)
WHERE
    nwg.IsDeleted = 0
    AND nwg.IsActive = 1
    AND nwg.UnitId = @UnitId
    AND (@SearchTerm IS NULL OR nwg.GroupName LIKE @SearchTerm)
ORDER BY nwg.GroupName ASC, nwg.Id DESC;";

            var param = new
            {
                UnitId = unitId,
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm.Trim()}%"
            };

            var list = await _db.QueryAsync<NotificationWhatsAppGroupAutoCompleteDto>(sql, param);
            return list.ToList();
        }

        public async Task<List<NotificationWhatsAppGroupAutoCompleteDto>> GetByDepartmentAsync(
            int departmentId,
            string? searchTerm,
            CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId();

            const string sql = @"
SELECT TOP 50
    nwg.Id,
    nwg.GroupName,
    nwg.DepartmentId,
    DepartmentName = CAST('' AS NVARCHAR(200))
FROM [AppNotification].[NotificationWhatsAppGroup] nwg WITH (NOLOCK)
WHERE
    nwg.IsDeleted = 0
    AND nwg.IsActive = 1
    AND nwg.UnitId = @UnitId
    AND nwg.DepartmentId = @DepartmentId
    AND (@SearchTerm IS NULL OR nwg.GroupName LIKE @SearchTerm)
ORDER BY nwg.GroupName ASC, nwg.Id DESC;";

            var param = new
            {
                UnitId = unitId,
                DepartmentId = departmentId,
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm.Trim()}%"
            };

            var list = await _db.QueryAsync<NotificationWhatsAppGroupAutoCompleteDto>(sql, param);
            return list.ToList();
        }
    }
}
