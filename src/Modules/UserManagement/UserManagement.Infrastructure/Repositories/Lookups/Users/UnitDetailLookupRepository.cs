using System.Data;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal sealed class UnitDetailLookupRepository : IUnitDetailLookup
    {
        private readonly IDbConnection _dbConnection;

        public UnitDetailLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<UnitDetailLookupDto?> GetByIdAsync(int unitId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT u.Id AS UnitId, u.UnitName, u.CINNO,
                    ua.AddressLine1, ua.AddressLine2,
                    ua.CityId, ua.StateId, ua.PinCode,
                    ua.ContactNumber AS Phone,
                    u.BankAccountId
                FROM AppData.Unit u
                LEFT JOIN AppData.UnitAddress ua ON ua.UnitId = u.Id
                WHERE u.Id = @UnitId AND u.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<UnitDetailLookupDto>(
                new CommandDefinition(sql, new { UnitId = unitId }, cancellationToken: ct));
        }
    }
}
