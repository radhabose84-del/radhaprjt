using System.Data;
using Dapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users;

internal sealed class MenuLookupRepository : IMenuLookup
{
    private readonly IDbConnection _dbConnection;

    public MenuLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<List<MenuLookupDto>> GetAllMenuAsync()
    {
        const string sql = @"
            SELECT Id AS MenuId, MenuName
            FROM [AppData].[Menus]
            WHERE IsDeleted = 0 AND IsActive = 1
            ORDER BY MenuName ASC;";

        var result = await _dbConnection.QueryAsync<MenuLookupDto>(sql);
        return result.ToList();
    }
}
