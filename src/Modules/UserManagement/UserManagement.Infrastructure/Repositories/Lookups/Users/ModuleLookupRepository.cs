using System.Data;
using Dapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users;

internal sealed class ModuleLookupRepository : IModuleLookup
{
    private readonly IDbConnection _dbConnection;

    public ModuleLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<List<ModuleLookupDto>> GetAllModuleAsync()
    {
        const string sql = @"
            SELECT Id AS ModuleId, ModuleName
            FROM [AppData].[Modules]
            WHERE IsDeleted = 0
            ORDER BY ModuleName ASC;";

        var result = await _dbConnection.QueryAsync<ModuleLookupDto>(sql);
        return result.ToList();
    }
}
