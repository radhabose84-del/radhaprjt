using System.Data;
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

            // Normalization: trim + lowercase + remove spaces (same as previous gRPC logic)
            static string Normalize(string input) =>
                string.IsNullOrWhiteSpace(input)
                    ? string.Empty
                    : input.Trim().ToLower().Replace(" ", "");

            static string Code(string name, int maxLen) =>
                name.Trim()[..Math.Min(maxLen, name.Trim().Length)].ToUpper();

            var normalizedCountry = Normalize(country);
            var normalizedState   = Normalize(state);
            var normalizedCity    = Normalize(city);

            // --- Step 1: Country ---
            const string countrySelectSql = @"
                SELECT Id, CountryName FROM [AppData].[Country]
                WHERE IsDeleted = 0 AND LOWER(REPLACE(TRIM(CountryName), ' ', '')) = @Normalized;";

            var (countryDbId, _) = await _dbConnection.QueryFirstOrDefaultAsync<(int Id, string CountryName)>(
                countrySelectSql, new { Normalized = normalizedCountry });

            int countryId;
            if (countryDbId > 0)
            {
                countryId = countryDbId;
            }
            else
            {
                const string countryInsertSql = @"
                    INSERT INTO [AppData].[Country]
                        (CountryCode, CountryName, IsActive, IsDeleted, CreatedBy, CreatedDate)
                    OUTPUT INSERTED.Id
                    VALUES (@CountryCode, @CountryName, 1, 0, 0, SYSDATETIMEOFFSET());";

                countryId = await _dbConnection.ExecuteScalarAsync<int>(countryInsertSql, new
                {
                    CountryCode = Code(country, 5),
                    CountryName = country.Trim()
                });
            }

            // --- Step 2: State ---
            const string stateSelectSql = @"
                SELECT Id, StateName FROM [AppData].[State]
                WHERE IsDeleted = 0
                  AND CountryId = @CountryId
                  AND LOWER(REPLACE(TRIM(StateName), ' ', '')) = @Normalized;";

            var (stateDbId, _) = await _dbConnection.QueryFirstOrDefaultAsync<(int Id, string StateName)>(
                stateSelectSql, new { CountryId = countryId, Normalized = normalizedState });

            int stateId;
            if (stateDbId > 0)
            {
                stateId = stateDbId;
            }
            else
            {
                const string stateInsertSql = @"
                    INSERT INTO [AppData].[State]
                        (StateCode, StateName, CountryId, IsActive, IsDeleted, CreatedBy, CreatedDate)
                    OUTPUT INSERTED.Id
                    VALUES (@StateCode, @StateName, @CountryId, 1, 0, 0, SYSDATETIMEOFFSET());";

                stateId = await _dbConnection.ExecuteScalarAsync<int>(stateInsertSql, new
                {
                    StateCode = Code(state, 5),
                    StateName = state.Trim(),
                    CountryId = countryId
                });
            }

            // --- Step 3: City ---
            const string citySelectSql = @"
                SELECT Id FROM [AppData].[City]
                WHERE IsDeleted = 0
                  AND StateId = @StateId
                  AND LOWER(REPLACE(TRIM(CityName), ' ', '')) = @Normalized;";

            var cityId = await _dbConnection.QueryFirstOrDefaultAsync<int?>(
                citySelectSql, new { StateId = stateId, Normalized = normalizedCity });

            if (cityId == null || cityId == 0)
            {
                const string cityInsertSql = @"
                    INSERT INTO [AppData].[City]
                        (CityCode, CityName, StateId, IsActive, IsDeleted, CreatedBy, CreatedDate)
                    OUTPUT INSERTED.Id
                    VALUES (@CityCode, @CityName, @StateId, 1, 0, 0, SYSDATETIMEOFFSET());";

                cityId = await _dbConnection.ExecuteScalarAsync<int>(cityInsertSql, new
                {
                    CityCode = Code(city, 5),
                    CityName = city.Trim(),
                    StateId  = stateId
                });
            }

            return new LocationLookupDto
            {
                CityId    = cityId.Value,
                StateId   = stateId,
                CountryId = countryId
            };
        }
    }
}
