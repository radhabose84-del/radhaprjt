using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IUserRole;
using System.Data;
using Dapper;
using Core.Application.UserRole.Queries.GetRole;
using Core.Application.Common.Interfaces;

namespace UserManagement.Infrastructure.Repositories.UserRoles
{
    public class UserRoleQueryRepository :IUserRoleQueryRepository
    {
        
         private readonly IDbConnection _dbConnection;  
         private readonly IIPAddressService _ipAddressService;       

    public  UserRoleQueryRepository(IDbConnection dbConnection,IIPAddressService ipAddressService)
    {
        _dbConnection = dbConnection;
        _ipAddressService = ipAddressService;
    }


            public async Task<(List<UserRole>, int)> GetAllRoleAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var CompanyId = _ipAddressService.GetCompanyId();
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM AppSecurity.UserRole 
                WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (RoleName LIKE @Search OR Description LIKE @Search)")}};
                
                SELECT 
                    Id, 
                    RoleName, 
                    Description, 
                    CompanyId, 
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
                FROM AppSecurity.UserRole 
                WHERE 
                    IsDeleted = 0 AND CompanyId=@CompanyId
                    {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (RoleName LIKE @Search OR Description LIKE @Search)")}}
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                
                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize,
                CompanyId
            };

            var roles = await _dbConnection.QueryMultipleAsync(query, parameters);
            var roleList = (await roles.ReadAsync<UserRole>()).ToList();
            int totalCount = (await roles.ReadFirstAsync<int>());

            return (roleList, totalCount);
        }


            public async Task<UserRole> GetByIdAsync(int id)
            {
                  
                var CompanyId = _ipAddressService.GetCompanyId();
                const string query = "SELECT Id,RoleName,Description,CompanyId,IsActive FROM  AppSecurity.UserRole WHERE Id = @Id AND IsDeleted=0 AND CompanyId=@CompanyId ORDER BY Id DESC";
                return await _dbConnection.QueryFirstOrDefaultAsync<UserRole>(query, new { id,CompanyId });
            
            }   
                public async Task<List<UserRole>> GetRolesAsync(string searchTerm = null)
            {
                var companyId = _ipAddressService.GetCompanyId();
                var userId = _ipAddressService.GetUserId();
                const string query = @"
                    SELECT U.Id, U.RoleName, U.Description
                    FROM AppSecurity.UserRole U
                    INNER JOIN [AppSecurity].[UserRoleAllocation] URA ON URA.UserRoleId=U.Id AND URA.IsActive=1
                    WHERE (U.RoleName LIKE @searchTerm OR CAST(U.Id AS NVARCHAR) LIKE @searchTerm) AND U.IsDeleted = 0 AND U.CompanyId=@CompanyId AND URA.UserId=@UserId
                    ORDER BY Id DESC";
                
                var parameters = new
                {
                    searchTerm = $"%{searchTerm ?? string.Empty}%",
                    CompanyId = companyId,
                    UserId = userId
                };

                var userRoles = await _dbConnection.QueryAsync<UserRole>(query, parameters);
                return userRoles.ToList();
            }
              public async Task<bool>SoftDeleteValidation(int Id)
            {
                                const string query = @"
                           SELECT 1 
                           FROM [AppSecurity].[RoleModule] 
                           WHERE RoleId = @Id ;
                    
                           SELECT 1 
                           FROM [AppSecurity].[RoleParent]
                           WHERE RoleId = @Id ;
                           
                            SELECT 1 
                           FROM [AppSecurity].[RoleChild]
                           WHERE RoleId = @Id ;
                           
                           SELECT 1 
                           FROM [AppSecurity].[RoleMenuPrivilege]
                           WHERE RoleId = @Id ;";
                    
                       using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });
                    
                       var RoleModuleExists = await multi.ReadFirstOrDefaultAsync<int?>();  
                       var RoleParentExists = await multi.ReadFirstOrDefaultAsync<int?>();
                       var RoleChildExists = await multi.ReadFirstOrDefaultAsync<int?>();
                       var RoleMenuPrivilegeExists = await multi.ReadFirstOrDefaultAsync<int?>();
                    
                       return RoleModuleExists.HasValue || RoleParentExists.HasValue || RoleChildExists.HasValue || RoleMenuPrivilegeExists.HasValue;
            }
            public async Task<bool> FKColumnExistValidation(int Id)
          {
              var sql = "SELECT COUNT(1) FROM AppSecurity.UserRole  WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";
                var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = Id });
                return count > 0;
          }
           public async Task<List<UserRole>> GetRoles_SuperAdmin(string searchTerm = null)
            {
                var companyId = _ipAddressService.GetCompanyId();
                const string query = @"
                    SELECT U.Id, U.RoleName, U.Description
                    FROM AppSecurity.UserRole U
                    
                    WHERE (U.RoleName LIKE @searchTerm OR CAST(U.Id AS NVARCHAR) LIKE @searchTerm) AND U.IsDeleted = 0 AND U.CompanyId=@CompanyId 
                    ORDER BY Id DESC";
                
                var parameters = new
                {
                    searchTerm = $"%{searchTerm ?? string.Empty}%",
                    CompanyId = companyId
                };

                var userRoles = await _dbConnection.QueryAsync<UserRole>(query, parameters);
                return userRoles.ToList();
            }

   
    }
}