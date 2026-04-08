using System.Data;
using Contracts.Interfaces.Lookups.Logistics;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Dto;

namespace SalesManagement.Infrastructure.Repositories.DispatchAddressMaster
{
    public class DispatchAddressMasterQueryRepository : IDispatchAddressMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICityLookup _cityLookup;
        private readonly IStateLookup _stateLookup;
        private readonly ICountryLookup _countryLookup;
        private readonly IFreightMasterLookup _freightMasterLookup;

        public DispatchAddressMasterQueryRepository(
            IDbConnection dbConnection,
            ICityLookup cityLookup,
            IStateLookup stateLookup,
            ICountryLookup countryLookup,
            IFreightMasterLookup freightMasterLookup)
        {
            _dbConnection = dbConnection;
            _cityLookup = cityLookup;
            _stateLookup = stateLookup;
            _countryLookup = countryLookup;
            _freightMasterLookup = freightMasterLookup;
        }

        public async Task<(List<DispatchAddressMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var cities = await _cityLookup.GetAllCityAsync();
            var states = await _stateLookup.GetAllStatesAsync();
            var countries = await _countryLookup.GetAllCountriesAsync();

            var cityDict = cities.ToDictionary(c => c.CityId, c => c.CityName);
            var stateDict = states.ToDictionary(s => s.StateId, s => s.StateName);
            var countryDict = countries.ToDictionary(c => c.CountryId, c => c.CountryName);

            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.DispatchAddressMaster dam
                WHERE dam.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (dam.DispatchAddressName LIKE @Search OR dam.AddressLine1 LIKE @Search)")}};

                SELECT dam.Id, dam.DispatchAddressName, dam.AddressLine1, dam.AddressLine2,
                    dam.CityId, dam.StateId, dam.CountryId, dam.PinCode,
                    dam.ContactPerson, dam.MobileNumber, dam.Email, dam.GSTIN,
                    dam.Latitude, dam.Longitude, dam.FreightId,
                    dam.IsActive, dam.IsDeleted,
                    dam.CreatedBy, dam.CreatedDate, dam.CreatedByName, dam.CreatedIP,
                    dam.ModifiedBy, dam.ModifiedDate, dam.ModifiedByName, dam.ModifiedIP
                FROM Sales.DispatchAddressMaster dam
                WHERE dam.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (dam.DispatchAddressName LIKE @Search OR dam.AddressLine1 LIKE @Search)")}}
                ORDER BY dam.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<DispatchAddressMasterDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            var freightList = await _freightMasterLookup.GetAllFreightMasterAsync();
            var freightDict = freightList.ToDictionary(f => f.Id);

            foreach (var item in list)
            {
                item.CityName = cityDict.TryGetValue(item.CityId, out var cn) ? cn : null;
                item.StateName = stateDict.TryGetValue(item.StateId, out var sn) ? sn : null;
                item.CountryName = countryDict.TryGetValue(item.CountryId, out var ctn) ? ctn : null;

                if (item.FreightId > 0 && freightDict.TryGetValue(item.FreightId, out var freight))
                {
                    item.FreightModeName = freight.FreightModeName;
                    item.RateMethodName = freight.RateMethodName;
                    item.FreightRate = freight.Rate;
                }
            }

            return (list, totalCount);
        }

        public async Task<DispatchAddressMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT dam.Id, dam.DispatchAddressName, dam.AddressLine1, dam.AddressLine2,
                    dam.CityId, dam.StateId, dam.CountryId, dam.PinCode,
                    dam.ContactPerson, dam.MobileNumber, dam.Email, dam.GSTIN,
                    dam.Latitude, dam.Longitude, dam.FreightId,
                    dam.IsActive, dam.IsDeleted,
                    dam.CreatedBy, dam.CreatedDate, dam.CreatedByName, dam.CreatedIP,
                    dam.ModifiedBy, dam.ModifiedDate, dam.ModifiedByName, dam.ModifiedIP
                FROM Sales.DispatchAddressMaster dam
                WHERE dam.Id = @Id AND dam.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<DispatchAddressMasterDto>(sql, new { Id = id });

            if (dto != null)
            {
                var city = await _cityLookup.GetByIdAsync(dto.CityId);
                var state = await _stateLookup.GetByIdAsync(dto.StateId);
                var country = await _countryLookup.GetByIdAsync(dto.CountryId);
                dto.CityName = city?.CityName;
                dto.StateName = state?.StateName;
                dto.CountryName = country?.CountryName;

                if (dto.FreightId > 0)
                {
                    var freight = await _freightMasterLookup.GetByIdAsync(dto.FreightId);
                    if (freight != null)
                    {
                        dto.FreightModeName = freight.FreightModeName;
                        dto.RateMethodName = freight.RateMethodName;
                        dto.FreightRate = freight.Rate;
                    }
                }
            }

            return dto;
        }

        public async Task<bool> CompositeKeyExistsAsync(string name, int cityId, string pinCode, int? excludeId = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Sales.DispatchAddressMaster
                WHERE DispatchAddressName = @Name
                AND CityId = @CityId
                AND PinCode = @PinCode
                AND IsDeleted = 0";

            if (excludeId.HasValue && excludeId.Value > 0)
                sql += " AND Id != @ExcludeId";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql,
                new { Name = name.Trim(), CityId = cityId, PinCode = pinCode.Trim(), ExcludeId = excludeId });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.DispatchAddressMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> CityExistsAsync(int cityId)
        {
            var cities = await _cityLookup.GetAllCityAsync();
            return cities.Any(c => c.CityId == cityId);
        }

        public async Task<bool> StateExistsAsync(int stateId)
        {
            var states = await _stateLookup.GetAllStatesAsync();
            return states.Any(s => s.StateId == stateId);
        }

        public async Task<bool> CountryExistsAsync(int countryId)
        {
            var countries = await _countryLookup.GetAllCountriesAsync();
            return countries.Any(c => c.CountryId == countryId);
        }

        public async Task<IReadOnlyList<DispatchAddressMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20 dam.Id, dam.DispatchAddressName, dam.AddressLine1, dam.CityId
                FROM Sales.DispatchAddressMaster dam
                WHERE dam.IsDeleted = 0 AND dam.IsActive = 1
                AND dam.DispatchAddressName LIKE @Term
                ORDER BY dam.DispatchAddressName ASC";

            var rows = (await _dbConnection.QueryAsync<DispatchAddressMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct))).ToList();

            var cities = await _cityLookup.GetAllCityAsync(ct);
            var cityDict = cities.ToDictionary(c => c.CityId, c => c.CityName);

            foreach (var row in rows)
                row.CityName = cityDict.TryGetValue(row.CityId, out var cn) ? cn : null;

            return rows;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // DispatchAddressMaster has no FK children — always safe to soft-delete.
            await Task.CompletedTask;
            return false;
        }
    }
}
