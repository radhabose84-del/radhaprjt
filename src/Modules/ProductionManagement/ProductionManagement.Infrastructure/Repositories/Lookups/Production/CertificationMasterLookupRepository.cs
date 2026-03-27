using System.Data;
using Contracts.Dtos.Lookups.Production;
using Contracts.Interfaces.Lookups.Production;
using Dapper;

namespace ProductionManagement.Infrastructure.Repositories.Lookups.Production
{
    internal sealed class CertificationMasterLookupRepository : ICertificationMasterLookup
    {
        private readonly IDbConnection _dbConnection;

        public CertificationMasterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<CertificationMasterLookupDto>> GetAllCertificationMasterAsync()
        {
            const string sql = @"
                SELECT Id, CertificationName
                FROM Production.CertificationMaster
                WHERE IsActive = 1 AND IsDeleted = 0
                ORDER BY CertificationName ASC;";

            var result = await _dbConnection.QueryAsync<CertificationMasterLookupDto>(sql);
            return result.ToList();
        }

        public async Task<IReadOnlyList<CertificationMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids?.Distinct().ToList();
            if (idList == null || idList.Count == 0)
                return new List<CertificationMasterLookupDto>();

            const string sql = @"
                SELECT Id, CertificationName
                FROM Production.CertificationMaster
                WHERE Id IN @Ids AND IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<CertificationMasterLookupDto>(sql, new { Ids = idList });
            return result.ToList();
        }
    }
}
