using System.Data;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Logistics;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.Lookups
{
    internal sealed class PartyLookupRepository : IPartyLookup
    {
        private readonly IDbConnection _dbConnection;
        private readonly IFreightMasterLookup _freightMasterLookup;
        private readonly ICityLookup _cityLookup;
        private readonly IStateLookup _stateLookup;
        private readonly ICountryLookup _countryLookup;

        public PartyLookupRepository(
            IDbConnection dbConnection,
            IFreightMasterLookup freightMasterLookup,
            ICityLookup cityLookup,
            IStateLookup stateLookup,
            ICountryLookup countryLookup)
        {
            _dbConnection = dbConnection;
            _freightMasterLookup = freightMasterLookup;
            _cityLookup = cityLookup;
            _stateLookup = stateLookup;
            _countryLookup = countryLookup;
        }

        public async Task<PartyLookupDto?> GetByIdAsync(int partyId, CancellationToken ct = default)
        {
            const string partySql = @"
                SELECT p.Id, p.PartyCode, p.PartyName, p.GSTNumber AS GstNumber,
                       p.SalesFreightId, p.PurchaseFreightId,
                       c.Email, c.Mobile
                FROM Party.PartyMaster p
                OUTER APPLY (
                    SELECT TOP 1
                        pc.EmailID AS Email,
                        pc.MobileNo AS Mobile
                    FROM Party.PartyContact pc
                    WHERE pc.PartyId = p.Id
                      AND pc.MobileNo IS NOT NULL
                      AND pc.MobileNo <> ''
                    ORDER BY pc.Id
                ) c
                WHERE p.Id = @PartyId;";

            var party = await _dbConnection.QueryFirstOrDefaultAsync<PartyLookupDto>(
                new CommandDefinition(partySql, new { PartyId = partyId }, cancellationToken: ct));

            if (party == null)
                return null;

            await PopulateFreightDetailsAsync([party]);
            await PopulateAddressesAsync([party], ct);

            return party;
        }

        public async Task<IReadOnlyList<PartyLookupDto>> GetByIdsAsync(IEnumerable<int> partyIds, CancellationToken ct = default)
        {
            var ids = partyIds?.ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<PartyLookupDto>();

            const string partySql = @"
                SELECT p.Id, p.PartyCode, p.PartyName, p.GSTNumber AS GstNumber,
                       p.SalesFreightId, p.PurchaseFreightId,
                       c.Email, c.Mobile
                FROM Party.PartyMaster p
                OUTER APPLY (
                    SELECT TOP 1
                        pc.EmailID AS Email,
                        pc.MobileNo AS Mobile
                    FROM Party.PartyContact pc
                    WHERE pc.PartyId = p.Id
                      AND pc.MobileNo IS NOT NULL
                      AND pc.MobileNo <> ''
                    ORDER BY pc.Id
                ) c
                WHERE p.Id IN @PartyIds;";

            var result = (await _dbConnection.QueryAsync<PartyLookupDto>(
                new CommandDefinition(partySql, new { PartyIds = ids }, cancellationToken: ct))).ToList();

            if (result.Count == 0)
                return result;

            await PopulateFreightDetailsAsync(result);
            await PopulateAddressesAsync(result, ct);

            return result;
        }

        private async Task PopulateFreightDetailsAsync(List<PartyLookupDto> parties)
        {
            var freightIds = parties
                .SelectMany(p => new[] { p.SalesFreightId, p.PurchaseFreightId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            if (freightIds.Count == 0)
                return;

            var allFreights = await _freightMasterLookup.GetAllFreightMasterAsync();
            var freightDict = allFreights
                .Where(f => freightIds.Contains(f.Id))
                .ToDictionary(f => f.Id);

            foreach (var party in parties)
            {
                if (party.SalesFreightId.HasValue && freightDict.TryGetValue(party.SalesFreightId.Value, out var salesFreight))
                    party.SalesFreight = salesFreight;

                if (party.PurchaseFreightId.HasValue && freightDict.TryGetValue(party.PurchaseFreightId.Value, out var purchaseFreight))
                    party.PurchaseFreight = purchaseFreight;
            }
        }

        private async Task PopulateAddressesAsync(List<PartyLookupDto> parties, CancellationToken ct)
        {
            var partyIds = parties.Select(p => p.Id).ToList();

            const string addressSql = @"
                SELECT Id, PartyId, AddressType, AddressLine1, AddressLine2,
                       PostalCode, CityId, StateId, CountryId
                FROM Party.PartyAddress
                WHERE PartyId IN @PartyIds;";

            var addresses = (await _dbConnection.QueryAsync<PartyAddressLookupDto>(
                new CommandDefinition(addressSql, new { PartyIds = partyIds }, cancellationToken: ct))).ToList();

            if (addresses.Count == 0)
            {
                foreach (var party in parties)
                    party.Addresses = [];
                return;
            }

            // Resolve City/State/Country names via cross-module lookups
            var cityIds = addresses.Where(a => a.CityId.HasValue).Select(a => a.CityId!.Value).Distinct();
            var stateIds = addresses.Where(a => a.StateId.HasValue).Select(a => a.StateId!.Value).Distinct();
            var countryIds = addresses.Where(a => a.CountryId.HasValue).Select(a => a.CountryId!.Value).Distinct();

            var cities = await _cityLookup.GetByIdsAsync(cityIds, ct);
            var states = await _stateLookup.GetByIdsAsync(stateIds, ct);
            var countries = await _countryLookup.GetByIdsAsync(countryIds, ct);

            var cityDict = cities.ToDictionary(c => c.CityId, c => c.CityName);
            var stateDict = states.ToDictionary(s => s.StateId, s => s.StateName);
            var countryDict = countries.ToDictionary(c => c.CountryId, c => c.CountryName);

            foreach (var addr in addresses)
            {
                if (addr.CityId.HasValue && cityDict.TryGetValue(addr.CityId.Value, out var cityName))
                    addr.City = cityName;
                if (addr.StateId.HasValue && stateDict.TryGetValue(addr.StateId.Value, out var stateName))
                    addr.State = stateName;
                if (addr.CountryId.HasValue && countryDict.TryGetValue(addr.CountryId.Value, out var countryName))
                    addr.Country = countryName;
            }

            var addressLookup = addresses.ToLookup(a => a.PartyId);

            foreach (var party in parties)
            {
                party.Addresses = addressLookup[party.Id].ToList();
            }
        }
    }
}
