using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers;
using BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetAllNotificationGroupMember;
using BackgroundService.Domain.Entities.Notification;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService.Infrastructure.Repositories.Notification.NotificationGroupMember
{
    public class NotificationGroupMemberQueryRepository : INotificationGroupMemberQuery
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
        public NotificationGroupMemberQueryRepository([FromKeyedServices("Notification")] IDbConnection dbConnection, IIPAddressService iPAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = iPAddressService;
        }
        public async Task<GetNotificationGroupMemberDto> GetByIdAsync(int id)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;

            const string query = @"
                SELECT  
                    NG.Id AS GroupId,
                    NG.GroupName,
                    ISNULL(NGM.UserId, 0) AS UserId,
                    U.UserName
                FROM [AppNotification].[NotificationGroup] NG
                LEFT JOIN [AppNotification].[NotificationGroupMembers] NGM 
                    ON NG.Id = NGM.GroupId AND NGM.IsDeleted = 0
                LEFT JOIN BannariERP.AppSecurity.Users U 
                    ON U.UserId = NGM.UserId
                WHERE NG.Id = @GroupId 
                AND NG.UnitId = @UnitId 
                AND NG.ISDeleted = 0 and NG.IsActive = 1;
            ";

            // Fetch data using a flat structure
            var result = await _dbConnection.QueryAsync<NotificationGroupFlatDto>(
                query,
                new { GroupId = id, UnitId }
            );

            if (!result.Any())
                return null;

            // Build NotificationGroupDto manually
            var groupDto = new GetNotificationGroupMemberDto
            {
                GroupId = result.First().GroupId,
                GroupName = result.First().GroupName,
                Users = result
                    .Where(r => r.UserId != 0)
                    .Select(r => new UserDto
                    {
                        UserId = r.UserId,
                        UserName = r.UserName
                    })
                    .ToList()
            };

            return groupDto;
        }

        public async Task<bool> AlreadyExistsAsync(int GroupId, int UserId, int? id = null)
        {
            var query = @"SELECT COUNT(1) 
                          FROM [AppNotification].[NotificationGroupMembers]
                          WHERE GroupId = @GroupId AND UserId = @UserId AND IsDeleted = 0";

            var parameters = new DynamicParameters(new { GroupId, UserId });

            if (id is not null)
            {
                query += " AND Id != @Id";
                parameters.Add("Id", id);
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
            return count > 0;
        }

        public async Task<(List<GetNotificationGroupMemberDto>, int)> GetAllNotificationGroupAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            const string dataQuery = @"
                SELECT  
                    NGM.GroupId,
                    NG.GroupName,
                    NGM.UserId,
                    U.UserName,NG.IsActive
                FROM [AppNotification].[NotificationGroupMembers] NGM
                INNER JOIN [AppNotification].[NotificationGroup] NG ON NG.Id = NGM.GroupId
                LEFT JOIN BannariERP.AppSecurity.Users U ON U.UserId = NGM.UserId
                WHERE NG.UnitId=@UnitId 
                AND NGM.IsDeleted = 0              
                AND (@Search IS NULL OR NG.GroupName LIKE @Search)
                ORDER BY NG.Id
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            ";

            const string countQuery = @"
                SELECT COUNT(DISTINCT NG.Id) 
                FROM [AppNotification].[NotificationGroupMembers] NGM
                INNER JOIN [AppNotification].[NotificationGroup] NG ON NG.Id = NGM.GroupId
                WHERE NG.UnitId=@UnitId AND NGM.IsDeleted = 0 
                AND (@Search IS NULL OR NG.GroupName LIKE @Search);
            ";

            var parameters = new
            {
                UnitId,
                Search = string.IsNullOrEmpty(searchTerm) ? null : $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var rawData = await _dbConnection.QueryAsync<NotificationGroupMemberDto>(
                dataQuery,
                parameters
            );

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countQuery, parameters);

            // ✅ Group users under their GroupId
            var groupedResult = rawData
                .GroupBy(x => new { x.GroupId, x.GroupName })
                .Select(g => new GetNotificationGroupMemberDto
                {
                    GroupId = g.Key.GroupId,
                    GroupName = g.Key.GroupName,
                    IsActive = g.First().IsActive,
                    Users = g.Select(u => new UserDto
                    {
                        UserId = u.UserId,
                        UserName = u.UserName
                    }).ToList()
                })
                .ToList();

            return (groupedResult, totalCount);
        }


        public async Task<bool> NotFoundAsync(int groupId)
        {
            var query = "SELECT COUNT(1) FROM [AppNotification].[NotificationGroupMembers] WHERE GroupId = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = groupId });
            return count > 0;
        }
        internal class NotificationGroupFlatDto
        {
            public int GroupId { get; set; }
            public string? GroupName { get; set; }
            public int UserId { get; set; }
            public string? UserName { get; set; }
        }
    }
}