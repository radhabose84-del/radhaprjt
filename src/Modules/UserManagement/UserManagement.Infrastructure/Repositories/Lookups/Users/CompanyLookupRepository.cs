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

        // US-GL02-10 (AC5) — companies the user is actively assigned to via AppSecurity.UserCompany.
        public async Task<List<CompanyLookupDto>> GetUserCompaniesAsync(int userId)
        {
            const string sql = @"
                SELECT
                    c.Id         AS CompanyId,
                    c.CompanyName,
                    c.LegalName,
                    c.GstNumber,
                    c.Tin TinNumber,
                    c.Tan TanNumber,
                    c.CSTNo CstNumber,
                    c.EntityId
                FROM [AppData].[Company] c
                INNER JOIN [AppSecurity].[UserCompany] uc ON uc.CompanyId = c.Id AND uc.IsActive = 1
                WHERE c.IsDeleted = 0 AND uc.UserId = @UserId
                ORDER BY c.CompanyName ASC;
            ";

            var result = await _dbConnection.QueryAsync<CompanyLookupDto>(sql, new { UserId = userId });
            return result.ToList();
        }
    }
}
