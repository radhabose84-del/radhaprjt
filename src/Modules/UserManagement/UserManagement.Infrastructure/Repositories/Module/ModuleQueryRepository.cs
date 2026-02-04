using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces.IModule;
using System.Data;
using Dapper;


namespace UserManagement.Infrastructure.Repositories.Module
{
    public class ModuleQueryRepository : IModuleQueryRepository
    {
        private readonly IDbConnection _dbConnection;


        public ModuleQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;

        }
        public async Task<Modules> GetModuleByIdAsync(int id)
        {
            var sql = @"
                SELECT 
                    m.Id, m.ModuleName
                FROM [AppData].[Modules] m
                WHERE m.Id = @ModuleId AND m.IsDeleted = 0
                ORDER BY m.Id";

            var moduleDictionary = new Dictionary<int, Modules>();

            var result = await _dbConnection.QueryAsync<Modules>(
                sql,
                new { ModuleId = id }
            );

            return result.FirstOrDefault();
            // return await _applicationDbContext.Modules.Include(m => m.Menus).FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(List<Modules>, int)> GetAllModulesAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM [AppData].[Modules] 
              WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ModuleName LIKE @Search )")}};

                SELECT 
                    Id, ModuleName
                FROM [AppData].[Modules] 
                WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (ModuleName LIKE @Search )")}}
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

            var modules = await _dbConnection.QueryMultipleAsync(query, parameters);
            var moduleslist = (await modules.ReadAsync<Modules>()).ToList();
            int totalCount = (await modules.ReadFirstAsync<int>());
            return (moduleslist, totalCount);

        }
        public async Task<List<Modules>> GetModule(string searchPattern)
        {


            var query = $@"
        SELECT Id, ModuleName 
        FROM AppData.Modules 
        WHERE IsDeleted = 0 
        AND ModuleName LIKE @SearchPattern";


            var parameters = new
            {
                SearchPattern = $"%{searchPattern ?? string.Empty}%"
            };

            var modules = await _dbConnection.QueryAsync<Modules>(query, parameters);
            return modules.ToList();
        }


        public async Task<bool> SoftDeleteValidation(int Id)
        {
            const string query = @"
                           SELECT 1 
                           FROM [AppData].[Menus] 
                    WHERE ModuleId = @Id AND   IsDeleted = 0;";

            using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });

            var MenuExists = await multi.ReadFirstOrDefaultAsync<int?>();

            return MenuExists.HasValue;
        }

        public async Task<Modules> GetModuleByNameAsync(string ModuleName)
        {
              var query = $@"
               SELECT Id, ModuleName 
               FROM AppData.Modules 
               WHERE IsDeleted = 0 
               AND ModuleName = @ModuleName";

            var modules = await _dbConnection.QueryAsync<Modules>(query, new { ModuleName });
            return modules.FirstOrDefault();
        }
    }
}