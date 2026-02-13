using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace WarehouseManagement.Infrastructure.Repositories.Lookups
{
    internal class LocationLookupRepository : ILocationLookup
    {
        private readonly IDbConnection _dbConnection;

        public LocationLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<LocationLookupDto?> GetLocationAsync(
            string city,
            string state,
            string country,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(state) ||
                string.IsNullOrWhiteSpace(country))
            {
                return null;
            }

            const string sql = @"
                SELECT TOP 1
                    C.Id AS CityId,
                    S.Id AS StateId,
                    COU.Id AS CountryId
                FROM [AppData].[City] C
                INNER JOIN [AppData].[State] S ON S.Id = C.StateId
                INNER JOIN [AppData].[Country] COU ON COU.Id = S.CountryId
                WHERE C.IsDeleted = 0
                  AND C.IsActive = 1
                  AND S.IsDeleted = 0
                  AND S.IsActive = 1
                  AND COU.IsDeleted = 0
                  AND COU.IsActive = 1
                  AND C.CityName = @CityName
                  AND S.StateName = @StateName
                  AND COU.CountryName = @CountryName;";

            var cmd = new CommandDefinition(
                sql,
                new
                {
                    CityName = city.Trim(),
                    StateName = state.Trim(),
                    CountryName = country.Trim()
                },
                cancellationToken: ct);

            return await _dbConnection.QueryFirstOrDefaultAsync<LocationLookupDto>(cmd);
        }
    }
}
