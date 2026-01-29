using System.Data;
using Core.Application.Common.Interfaces;
using Core.Application.Common.Interfaces.IUserGroup;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.UserGroup
{
    public class UserGroupQueryRepository :IUserGroupQueryRepository
    {
         private readonly IDbConnection _dbConnection; 
         private readonly IIPAddressService _ipAddressService;
        public  UserGroupQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<Core.Domain.Entities.UserGroup>, int)> GetAllUserGroupAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
               var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM AppSecurity.UserGroup  
                WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (GroupCode LIKE @Search OR GroupName LIKE @Search)")}};

                SELECT Id,GroupCode, GroupName, IsActive ,CreatedBy,CreatedAt,CreatedByName,CreatedIP,ModifiedBy,ModifiedAt,ModifiedByName,ModifiedIP
                FROM AppSecurity.UserGroup WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (GroupCode LIKE @Search OR GroupName LIKE @Search )")}}
                ORDER BY Id desc
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                SELECT @TotalCount AS TotalCount;
                """;
            var parameters = new
                       {
                           Search = $"%{SearchTerm}%",
                           Offset = (PageNumber - 1) * PageSize,
                           PageSize
                       };          
            var userGroup = await _dbConnection.QueryMultipleAsync(query, parameters);
            var userGroupList = (await userGroup.ReadAsync<Core.Domain.Entities.UserGroup>()).ToList();
            int totalCount = (await userGroup.ReadFirstAsync<int>());             
            return (userGroupList, totalCount);   
        }

        public async Task<Core.Domain.Entities.UserGroup> GetByIdAsync(int id)
        {
            const string query = @"
            SELECT Id, GroupCode, GroupName, IsActive, CreatedBy, CreatedAt, CreatedByName, CreatedIP, 
            ModifiedBy, ModifiedAt, ModifiedByName, ModifiedIP
            FROM AppSecurity.UserGroup  
            WHERE Id = @id AND IsDeleted = 0";
            var userGroup = await _dbConnection.QueryFirstOrDefaultAsync<Core.Domain.Entities.UserGroup>(query, new { id });           
             if (userGroup is null)
            {
                throw new KeyNotFoundException($"UserGroup with ID {id} not found.");
            }
            return userGroup;
        }

        public async Task<List<Core.Domain.Entities.UserGroup>> GetUserGroups(string searchTerm = null)
        { 
            var groupCode = _ipAddressService.GetGroupcode();
            var userid = _ipAddressService.GetUserId();
            var query="";
            if (groupCode == "ADMIN" )
            {
                query = @"
            SELECT Id, GroupCode, GroupName,  IsActive, CreatedBy, CreatedAt, CreatedByName, CreatedIP, 
                ModifiedBy, ModifiedAt, ModifiedByName, ModifiedIP, IsDeleted
            FROM AppSecurity.UserGroup
            WHERE (GroupName LIKE @searchTerm OR CAST(Id AS NVARCHAR) LIKE @searchTerm) AND IsDeleted = 0 AND GroupCode IN ('ADMIN','USER')
            ORDER BY Id DESC";
            }
             else if (groupCode == "USER")
            {
                query = @"Declare @Groupid int
            SET @Groupid =(SELECT UserGroupId FROM AppSecurity.Users WHERE UserId=@UserId)
            SELECT Id, GroupCode, GroupName,  IsActive, CreatedBy, CreatedAt, CreatedByName, CreatedIP, 
                ModifiedBy, ModifiedAt, ModifiedByName, ModifiedIP, IsDeleted
            FROM AppSecurity.UserGroup
            WHERE (GroupName LIKE @searchTerm OR CAST(Id AS NVARCHAR) LIKE @searchTerm) AND IsDeleted = 0 AND GroupCode IN ('USER') AND Id =@Groupid
            ORDER BY Id DESC";
            }
            else if (groupCode == "SUPER_ADMIN" )
            {
                query = @"
            SELECT Id, GroupCode, GroupName,  IsActive, CreatedBy, CreatedAt, CreatedByName, CreatedIP, 
                ModifiedBy, ModifiedAt, ModifiedByName, ModifiedIP, IsDeleted
            FROM AppSecurity.UserGroup
            WHERE (GroupName LIKE @searchTerm OR CAST(Id AS NVARCHAR) LIKE @searchTerm) AND IsDeleted = 0 
            ORDER BY Id DESC";
            }
             
            

            var parameters = new
            {
            searchTerm = $"%{searchTerm ?? string.Empty}%",
            UserId =userid
            };

            var userRoles = await _dbConnection.QueryAsync<Core.Domain.Entities.UserGroup>(query, parameters);
            return userRoles.ToList();
        }

        public async Task<bool> SoftDeleteValidation(int Id)
        {
            const string query = @"
                    SELECT 1 
                    FROM  AppSecurity.Users
                    WHERE UserGroupId = @Id AND   IsDeleted = 0;";            
            using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });            
            var userGroupExists = await multi.ReadFirstOrDefaultAsync<int?>();              
            return userGroupExists.HasValue ;
        }
    }
}