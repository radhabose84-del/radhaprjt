using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IMenu;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Menu
{
    public class MenuQueryRepository : IMenuQuery
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
        public MenuQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<List<UserManagement.Domain.Entities.Menu>> GetChildMenus(List<int> parentId)
        {
            // var companyId = _ipAddressService.GetCompanyId();
            string parentIdList = string.Join(",", parentId);
            string query = $@"
              WITH RecursiveMenu AS (
                  SELECT Id, ModuleId, ParentId, MenuName, MenuUrl, Type
                  FROM [AppData].[Menus]
                  WHERE ParentId IN ({parentIdList}) AND IsDeleted=0
                  UNION ALL
                  SELECT m.Id, m.ModuleId, m.ParentId, m.MenuName, m.MenuUrl, m.Type
                  FROM [AppData].[Menus] m
                  INNER JOIN RecursiveMenu rm ON m.ParentId = rm.Id
              )
              SELECT * FROM RecursiveMenu;";

            var childMenus = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.Menu>(query);
            return childMenus.ToList();
        }

        public async Task<List<UserManagement.Domain.Entities.Menu>> GetParentMenus(List<int> moduleId)
        {
            // var companyId = _ipAddressService.GetCompanyId();

            string moduleIdList = string.Join(",", moduleId);
            string query = $@"
                                  SELECT Id, MenuName 
                                  FROM AppData.Menus 
                                  WHERE IsDeleted = 0 AND ModuleId IN ({moduleIdList}) AND ParentId = 0 
                                  ";

            var parentMenus = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.Menu>(query);
            return parentMenus.ToList();
        }
        public async Task<bool> FKColumnExistValidation(int Id)
        {
            // var companyId = _ipAddressService.GetCompanyId();
            var sql = "SELECT COUNT(1) FROM AppData.Menus WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1 ";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = Id });
            return count > 0;
        }
        public async Task<(IEnumerable<dynamic>, int)> GetAllMenuAsync(int PageNumber, int PageSize, string? SearchTerm)
        {

            var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM AppData.Menus ME
            INNER JOIN [AppData].[Modules] MO ON MO.Id=ME.ModuleId
            LEFT JOIN AppData.Menus ParentMenu ON ParentMenu.Id=ME.ParentId
              WHERE ME.IsDeleted = 0 
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ME.MenuName LIKE @Search OR MO.ModuleName LIKE @Search)")}};

                SELECT
                ME.Id , 
                ME.ModuleId,
                MO.ModuleName,
                ME.MenuName , 
                ME.MenuIcon,
                ME.MenuUrl,
                ME.ParentId,
                ParentMenu.MenuName AS ParentName,
                ME.SortOrder,
                Cast(ME.CreatedAt as varchar) AS CreatedAt,
                ME.Type
            FROM AppData.Menus ME
            INNER JOIN [AppData].[Modules] MO ON MO.Id=ME.ModuleId
            LEFT JOIN AppData.Menus ParentMenu ON ParentMenu.Id=ME.ParentId
            WHERE 
            ME.IsDeleted = 0 
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ME.MenuName LIKE @Search OR MO.ModuleName LIKE @Search )")}}
                ORDER BY ME.Id desc
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;


            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

            var MenuMaster = await _dbConnection.QueryMultipleAsync(query, parameters);
            var MenuMasterList = await MenuMaster.ReadAsync<dynamic>();
            int TotalCount = (await MenuMaster.ReadFirstAsync<int>());

            return (MenuMasterList, TotalCount);
        }
        public async Task<UserManagement.Domain.Entities.Menu> GetMenuByNameAsync(string MenuName)
        {
            var query = $@"
               SELECT Id, MenuName 
               FROM AppData.Menus 
               WHERE IsDeleted = 0 
               AND MenuName = @MenuName";

            var Menus = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.Menu>(query, new { MenuName });
            return Menus.FirstOrDefault();
        }
        public async Task<List<UserManagement.Domain.Entities.Menu>> GetParentMenuAutoComplete(string searchPattern)
        {


            var query = $@"
                 SELECT Id, MenuName 
                 FROM AppData.Menus 
                 WHERE IsDeleted = 0 
                 AND MenuName LIKE @SearchPattern";


            var parameters = new
            {
                SearchPattern = $"%{searchPattern ?? string.Empty}%"
            };

            var modules = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.Menu>(query, parameters);
            return modules.ToList();
        }
        
         public async Task<List<UserManagement.Domain.Entities.Menu>> GetMenusByIds(IEnumerable<int> ids, CancellationToken ct = default)
        {
                var list = ids?.Distinct().ToList() ?? new();

                     var query = $@"
                 SELECT Id, MenuName 
                 FROM AppData.Menus 
                 WHERE IsDeleted = 0 
                 AND Id IN @Ids";


                  var parameters = new
                  {
                      Ids = list
                  };
        
                  var menus = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.Menu>(query, parameters);
                  return menus.ToList();
        }
    }
}