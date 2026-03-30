using System.Data;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal sealed class CompanyDetailLookupRepository : ICompanyDetailLookup
    {
        private readonly IDbConnection _dbConnection;

        public CompanyDetailLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<CompanyDetailLookupDto?> GetByUnitIdAsync(int unitId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT c.Id AS CompanyId, c.CompanyName, c.LegalName,
                    c.GstNumber, c.PanNumber, c.Website,
                    ca.AddressLine1, ca.AddressLine2,
                    ca.CityId, ca.StateId, ca.PinCode,
                    ca.Phone, cc.Email
                FROM AppData.Unit u
                INNER JOIN AppData.Company c ON u.CompanyId = c.Id AND c.IsDeleted = 0
                LEFT JOIN AppData.CompanyAddress ca ON ca.CompanyId = c.Id
                LEFT JOIN AppData.CompanyContact cc ON cc.CompanyId = c.Id
                WHERE u.Id = @UnitId AND u.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<CompanyDetailLookupDto>(
                new CommandDefinition(sql, new { UnitId = unitId }, cancellationToken: ct));
        }
    }
}
