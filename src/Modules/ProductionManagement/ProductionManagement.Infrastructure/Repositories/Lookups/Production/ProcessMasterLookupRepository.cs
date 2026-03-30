using System.Data;
using Contracts.Dtos.Lookups.Production;
using Contracts.Interfaces.Lookups.Production;
using Dapper;

namespace ProductionManagement.Infrastructure.Repositories.Lookups.Production
{
    internal sealed class ProcessMasterLookupRepository : IProcessMasterLookup
    {
        private readonly IDbConnection _dbConnection;

        public ProcessMasterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<ProcessMasterLookupDto>> GetAllProcessMasterAsync()
        {
            const string sql = @"
                SELECT Id, ProcessName
                FROM Production.ProcessMaster
                WHERE IsActive = 1 AND IsDeleted = 0
                ORDER BY ProcessName ASC;";

            var result = await _dbConnection.QueryAsync<ProcessMasterLookupDto>(sql);
            return result.ToList();
        }

        public async Task<IReadOnlyList<ProcessMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids?.Distinct().ToList();
            if (idList == null || idList.Count == 0)
                return new List<ProcessMasterLookupDto>();

            const string sql = @"
                SELECT Id, ProcessName
                FROM Production.ProcessMaster
                WHERE Id IN @Ids AND IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<ProcessMasterLookupDto>(sql, new { Ids = idList });
            return result.ToList();
        }
    }
}
