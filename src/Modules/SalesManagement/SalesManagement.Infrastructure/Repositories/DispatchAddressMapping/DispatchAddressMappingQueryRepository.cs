using System.Data;
using Contracts.Interfaces.Lookups.Logistics;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Dto;

namespace SalesManagement.Infrastructure.Repositories.DispatchAddressMapping
{
    public class DispatchAddressMappingQueryRepository : IDispatchAddressMappingQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IFreightMasterLookup _freightMasterLookup;
        private readonly ICityLookup _cityLookup;
        private readonly IStateLookup _stateLookup;
        private readonly ICountryLookup _countryLookup;

        public DispatchAddressMappingQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IFreightMasterLookup freightMasterLookup,
            ICityLookup cityLookup,
            IStateLookup stateLookup,
            ICountryLookup countryLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _freightMasterLookup = freightMasterLookup;
            _cityLookup = cityLookup;
            _stateLookup = stateLookup;
            _countryLookup = countryLookup;
        }

        public async Task<(List<DispatchAddressMappingDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm, int? partyId = null)
        {
            var whereClause = "dam.IsDeleted = 0";
            if (partyId.HasValue && partyId.Value > 0)
                whereClause += " AND dam.PartyId = @PartyId";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND damaster.DispatchAddressName LIKE @Search";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.DispatchAddressMapping dam
                LEFT JOIN Sales.DispatchAddressMaster damaster ON dam.DispatchAddressId = damaster.Id AND damaster.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON dam.UsageTypeId = mm.Id AND mm.IsDeleted = 0
                WHERE {whereClause};

                SELECT dam.Id, dam.PartyId, dam.DispatchAddressId, dam.UsageTypeId, dam.IsDefault,
                    damaster.DispatchAddressName, damaster.AddressLine1 AS DispatchAddressLine1,
                    damaster.FreightId,
                    damaster.CityId, damaster.StateId, damaster.CountryId,
                    damaster.PinCode, damaster.ContactPerson, damaster.MobileNumber,
                    damaster.Email, damaster.GSTIN,
                    damaster.Latitude, damaster.Longitude,
                    mm.Description AS UsageTypeName,
                    dam.IsActive, dam.IsDeleted,
                    dam.CreatedBy, dam.CreatedDate, dam.CreatedByName, dam.CreatedIP,
                    dam.ModifiedBy, dam.ModifiedDate, dam.ModifiedByName, dam.ModifiedIP
                FROM Sales.DispatchAddressMapping dam
                LEFT JOIN Sales.DispatchAddressMaster damaster ON dam.DispatchAddressId = damaster.Id AND damaster.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON dam.UsageTypeId = mm.Id AND mm.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY dam.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                PartyId = partyId,
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<DispatchAddressMappingDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                var partyIds = list.Select(x => x.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                foreach (var item in list)
                    item.PartyName = partyDict.TryGetValue(item.PartyId, out var name) ? name : null;

                // Populate City/State/Country names via cross-module lookups
                var cities = await _cityLookup.GetAllCityAsync();
                var states = await _stateLookup.GetAllStatesAsync();
                var countries = await _countryLookup.GetAllCountriesAsync();

                var cityDict = cities.ToDictionary(c => c.CityId, c => c.CityName);
                var stateDict = states.ToDictionary(s => s.StateId, s => s.StateName);
                var countryDict = countries.ToDictionary(c => c.CountryId, c => c.CountryName);

                foreach (var item in list)
                {
                    if (item.CityId.HasValue)
                        item.CityName = cityDict.TryGetValue(item.CityId.Value, out var cn) ? cn : null;
                    if (item.StateId.HasValue)
                        item.StateName = stateDict.TryGetValue(item.StateId.Value, out var sn) ? sn : null;
                    if (item.CountryId.HasValue)
                        item.CountryName = countryDict.TryGetValue(item.CountryId.Value, out var ctn) ? ctn : null;
                }
            }

            // Populate freight details via cross-module lookup
            var freightIds = list.Where(x => x.FreightId > 0).Select(x => x.FreightId).Distinct().ToList();
            if (freightIds.Count > 0)
            {
                var freightList = await _freightMasterLookup.GetAllFreightMasterAsync();
                var freightDict = freightList.ToDictionary(f => f.Id);

                foreach (var item in list.Where(x => x.FreightId > 0))
                {
                    if (freightDict.TryGetValue(item.FreightId, out var freight))
                    {
                        item.FreightModeName = freight.FreightModeName;
                        item.RateMethodName = freight.RateMethodName;
                        item.FreightRate = freight.Rate;
                    }
                }
            }

            return (list, totalCount);
        }

        public async Task<DispatchAddressMappingDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT dam.Id, dam.PartyId, dam.DispatchAddressId, dam.UsageTypeId, dam.IsDefault,
                    damaster.DispatchAddressName, damaster.AddressLine1 AS DispatchAddressLine1,
                    damaster.FreightId,
                    damaster.CityId, damaster.StateId, damaster.CountryId,
                    damaster.PinCode, damaster.ContactPerson, damaster.MobileNumber,
                    damaster.Email, damaster.GSTIN,
                    damaster.Latitude, damaster.Longitude,
                    mm.Description AS UsageTypeName,
                    dam.IsActive, dam.IsDeleted,
                    dam.CreatedBy, dam.CreatedDate, dam.CreatedByName, dam.CreatedIP,
                    dam.ModifiedBy, dam.ModifiedDate, dam.ModifiedByName, dam.ModifiedIP
                FROM Sales.DispatchAddressMapping dam
                LEFT JOIN Sales.DispatchAddressMaster damaster ON dam.DispatchAddressId = damaster.Id AND damaster.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster mm ON dam.UsageTypeId = mm.Id AND mm.IsDeleted = 0
                WHERE dam.Id = @Id AND dam.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<DispatchAddressMappingDto>(sql, new { Id = id });

            if (dto != null)
            {
                var party = await _partyLookup.GetByIdAsync(dto.PartyId);
                dto.PartyName = party?.PartyName;

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

                // Populate City/State/Country names via cross-module lookups
                if (dto.CityId.HasValue)
                {
                    var city = await _cityLookup.GetByIdAsync(dto.CityId.Value);
                    dto.CityName = city?.CityName;
                }
                if (dto.StateId.HasValue)
                {
                    var state = await _stateLookup.GetByIdAsync(dto.StateId.Value);
                    dto.StateName = state?.StateName;
                }
                if (dto.CountryId.HasValue)
                {
                    var country = await _countryLookup.GetByIdAsync(dto.CountryId.Value);
                    dto.CountryName = country?.CountryName;
                }
            }

            return dto;
        }

        public async Task<IReadOnlyList<DispatchAddressMappingLookupDto>> AutocompleteAsync(
            string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20 dam.Id, dam.PartyId, dam.DispatchAddressId, damaster.DispatchAddressName
                FROM Sales.DispatchAddressMapping dam
                LEFT JOIN Sales.DispatchAddressMaster damaster ON dam.DispatchAddressId = damaster.Id AND damaster.IsDeleted = 0
                WHERE dam.IsDeleted = 0 AND dam.IsActive = 1
                AND damaster.DispatchAddressName LIKE @Term
                ORDER BY damaster.DispatchAddressName ASC";

            var rows = (await _dbConnection.QueryAsync<AutocompleteRow>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct))).ToList();

            if (rows.Count == 0)
                return new List<DispatchAddressMappingLookupDto>();

            var partyIds = rows.Select(r => r.PartyId).Distinct();
            var parties = await _partyLookup.GetByIdsAsync(partyIds, ct);
            var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

            return rows.Select(r => new DispatchAddressMappingLookupDto
            {
                Id = r.Id,
                DispatchAddressId = r.DispatchAddressId,
                DispatchAddressName = r.DispatchAddressName,
                PartyName = partyDict.TryGetValue(r.PartyId, out var name) ? name : null
            }).ToList();
        }

        public async Task<bool> CompositeKeyExistsAsync(
            int partyId, int dispatchAddressId, int usageTypeId, int? excludeId = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Sales.DispatchAddressMapping
                WHERE PartyId = @PartyId
                AND DispatchAddressId = @DispatchAddressId
                AND UsageTypeId = @UsageTypeId
                AND IsDeleted = 0";

            if (excludeId.HasValue && excludeId.Value > 0)
                sql += " AND Id != @ExcludeId";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql,
                new { PartyId = partyId, DispatchAddressId = dispatchAddressId, UsageTypeId = usageTypeId, ExcludeId = excludeId });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.DispatchAddressMapping
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> DispatchAddressExistsAsync(int dispatchAddressId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.DispatchAddressMaster
                WHERE Id = @DispatchAddressId AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { DispatchAddressId = dispatchAddressId });
            return count > 0;
        }

        public async Task<bool> MiscMasterExistsAsync(int usageTypeId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MiscMaster
                WHERE Id = @UsageTypeId AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { UsageTypeId = usageTypeId });
            return count > 0;
        }

        public async Task<bool> DefaultAlreadyExistsAsync(int partyId, int usageTypeId, int? excludeId = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Sales.DispatchAddressMapping
                WHERE PartyId = @PartyId
                AND UsageTypeId = @UsageTypeId
                AND IsDefault = 1
                AND IsDeleted = 0";

            if (excludeId.HasValue && excludeId.Value > 0)
                sql += " AND Id != @ExcludeId";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql,
                new { PartyId = partyId, UsageTypeId = usageTypeId, ExcludeId = excludeId });
            return count > 0;
        }

        private sealed class AutocompleteRow
        {
            public int Id { get; set; }
            public int PartyId { get; set; }
            public int DispatchAddressId { get; set; }
            public string? DispatchAddressName { get; set; }
        }
    }
}
