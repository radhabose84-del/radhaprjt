using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Projects;
using Contracts.Interfaces.Lookups.Projects;
using Dapper;

namespace BudgetManagement.Infrastructure.Repositories.Lookups.Projects;

internal class ProjectWbsLookupRepository : IProjectWbsLookup
{
    private readonly IDbConnection _dbConnection;

    public ProjectWbsLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IReadOnlyList<ProjectWbsLookupDto>> GetByIdsAsync(IEnumerable<int> wbsIds, CancellationToken ct = default)
    {
        var ids = wbsIds?
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        if (ids == null || ids.Length == 0)
            return Array.Empty<ProjectWbsLookupDto>();

        const string sql = @"
            SELECT Id                         AS WbsId,
                   ProjectId,
                   WorkBreakdownStructureName
            FROM [Project].[ProjectWbs]
            WHERE IsDeleted = 0
              AND Id IN @WbsIds;
        ";

        var result = await _dbConnection.QueryAsync<ProjectWbsLookupDto>(
            new CommandDefinition(
                sql,
                new { WbsIds = ids },
                cancellationToken: ct));

        return result.ToList();
    }
}
