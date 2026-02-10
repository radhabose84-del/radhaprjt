using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Projects;
using Contracts.Interfaces.Lookups.Projects;
using Dapper;

namespace ProjectManagement.Infrastructure.Repositories.Lookups.Projects;

internal class ProjectLookupRepository : IProjectLookup
{
    private readonly IDbConnection _dbConnection;

    public ProjectLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IReadOnlyList<ProjectLookupDto>> GetByIdsAsync(IEnumerable<int> projectIds, CancellationToken ct = default)
    {
        var ids = projectIds?
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        if (ids == null || ids.Length == 0)
            return Array.Empty<ProjectLookupDto>();

        const string sql = @"
            SELECT Id     AS ProjectId,
                   ProjectName,
                   ProjectCode
            FROM [Project].[ProjectMaster]
            WHERE IsDeleted = 0
              AND Id IN @ProjectIds;
        ";

        var result = await _dbConnection.QueryAsync<ProjectLookupDto>(
            new CommandDefinition(
                sql,
                new { ProjectIds = ids },
                cancellationToken: ct));

        return result.ToList();
    }
}
