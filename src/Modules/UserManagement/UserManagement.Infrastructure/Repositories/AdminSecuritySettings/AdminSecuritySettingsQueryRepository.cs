using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using System.Data;
using Dapper;
using Core.Application.Common.Interfaces.IAdminSecuritySettings;
using System.Security.Permissions;
using Core.Application.Common.Interfaces;


namespace UserManagement.Infrastructure.Repositories.AdminSecuritySettings
{
    public class AdminSecuritySettingsQueryRepository  :IAdminSecuritySettingsQueryRepository
    {
        private readonly IDbConnection _dbConnection; 
        private readonly IIPAddressService _ipAddressService;

        public  AdminSecuritySettingsQueryRepository(IDbConnection dbConnection,IIPAddressService iPAddressService)
    {
         _dbConnection = dbConnection;
         _ipAddressService = iPAddressService;
    }
   
            public async Task<(List<Core.Domain.Entities.AdminSecuritySettings>, int)> GetAllAdminSecuritySettingsAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var entityId = _ipAddressService.GetEntityId();
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM AppSecurity.AdminSecuritySettings
                WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Id LIKE @Search)")}};
                
                SELECT Id,
                    PasswordHistoryCount,
                    SessionTimeoutMinutes,
                    MaxFailedLoginAttempts,
                    AccountAutoUnlockMinutes,
                    PasswordExpiryDays,
                    PasswordExpiryAlertDays,
                    IsTwoFactorAuthenticationEnabled,
                    MaxConcurrentLogins,
                    IsForcePasswordChangeOnFirstLogin,
                    PasswordResetCodeExpiryMinutes,
                    IsCaptchaEnabledOnLogin,
                    IsActive,
                    CreatedBy,
                    CreatedAt,
                    CreatedByName,
                    CreatedIP,
                    ModifiedBy,
                    ModifiedAt,
                    ModifiedByName,
                    ModifiedIP,
                    IsDeleted
                FROM AppSecurity.AdminSecuritySettings
                WHERE IsDeleted = 0 AND EntityId=@EntityId
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Id LIKE @Search)")}}
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                
                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize,
                EntityId =entityId
            };

            var securitySettings = await _dbConnection.QueryMultipleAsync(query, parameters);
            var settingsList = (await securitySettings.ReadAsync<Core.Domain.Entities.AdminSecuritySettings>()).ToList();
            int totalCount = (await securitySettings.ReadFirstAsync<int>());

            return (settingsList, totalCount);
        }


        public async Task<Core.Domain.Entities.AdminSecuritySettings> GetAdminSecuritySettingsByIdAsync(int id)
        {
            var entityId = _ipAddressService.GetEntityId();
            const string query = @"SELECT * FROM AppSecurity.AdminSecuritySettings WHERE Id = @Id AND IsDeleted = 0 AND EntityId=@EntityId ORDER BY ID DESC";
                var adminsettings = await _dbConnection.QueryFirstOrDefaultAsync<Core.Domain.Entities.AdminSecuritySettings>(query, new { Id=id,EntityId=entityId });
                
                if (adminsettings == null)
                {
                    throw new KeyNotFoundException($"Admin Security Settings with ID {id} not found.");
                }

                return adminsettings;
        }



    }
}