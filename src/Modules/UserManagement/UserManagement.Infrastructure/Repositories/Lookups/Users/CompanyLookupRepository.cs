using System.Data;
using Dapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    /// <summary>
    /// Read-only lookup repository for company data owned by UserManagement.
    /// </summary>
    internal class CompanyLookupRepository : ICompanyLookup
    {
        private readonly IDbConnection _dbConnection;

        public CompanyLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<CompanyLookupDto>> GetAllCompanyAsync()
        {
            const string sql = @"
                SELECT
                    Id         AS CompanyId,
                    CompanyName,
                    LegalName,
                    GstNumber,
                    Tin TinNumber,
                    Tan TanNumber,
                    CSTNo CstNumber,
                    EntityId
                FROM [AppData].[Company]
                WHERE IsDeleted = 0
                ORDER BY CompanyName ASC;
            ";

            var result = await _dbConnection.QueryAsync<CompanyLookupDto>(sql);
            return result.ToList();
        }
    }
}
