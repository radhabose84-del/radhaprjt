using System.Data;
using Contracts.Interfaces;
using Contracts.Dtos.Lookups.Budget;
using Contracts.Interfaces.Lookups.Budget;
using Dapper;

namespace BudgetManagement.Infrastructure.Repositories.Lookups.Budget;

internal sealed class BudgetGroupLookupRepository : IBudgetGroupLookup
{
    private readonly IDbConnection _dbConnection;
    private readonly IIPAddressService _ipAddressService;

    public BudgetGroupLookupRepository(
        IDbConnection dbConnection,
        IIPAddressService ipAddressService)
    {
        _dbConnection = dbConnection;
        _ipAddressService = ipAddressService;
    }

    public async Task<IReadOnlyList<BudgetGroupLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var distinctIds = ids
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        if (distinctIds.Length == 0)
            return Array.Empty<BudgetGroupLookupDto>();

        const string sql = @"
            SELECT Id, Name
            FROM Budget.BudgetGroup
            WHERE Id IN @Ids
              AND IsDeleted = 0
              AND IsActive = 1
              AND UnitId = @UnitId;";

        var result = await _dbConnection.QueryAsync<BudgetGroupLookupDto>(
            new CommandDefinition(sql, new { Ids = distinctIds, UnitId = _ipAddressService.GetUnitId() ?? 0 }, cancellationToken: ct));

        return result.ToList();
    }
}
