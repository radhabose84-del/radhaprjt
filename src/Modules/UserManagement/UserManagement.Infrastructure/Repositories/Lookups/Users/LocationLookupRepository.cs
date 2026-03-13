using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using UserManagement.Application.Common.Interfaces.ICity;
using UserManagement.Application.Common.Interfaces.ICountry;
using UserManagement.Application.Common.Interfaces.IState;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal sealed class LocationLookupRepository : ILocationLookup
    {
        private readonly ICountryQueryRepository   _countryQueryRepository;
        private readonly ICountryCommandRepository _countryCommandRepository;
        private readonly IStateQueryRepository     _stateQueryRepository;
        private readonly IStateCommandRepository   _stateCommandRepository;
        private readonly ICityQueryRepository      _cityQueryRepository;
        private readonly ICityCommandRepository    _cityCommandRepository;

        public LocationLookupRepository(
            ICountryQueryRepository   countryQueryRepository,
            ICountryCommandRepository countryCommandRepository,
            IStateQueryRepository     stateQueryRepository,
            IStateCommandRepository   stateCommandRepository,
            ICityQueryRepository      cityQueryRepository,
            ICityCommandRepository    cityCommandRepository)
        {
            _countryQueryRepository   = countryQueryRepository;
            _countryCommandRepository = countryCommandRepository;
            _stateQueryRepository     = stateQueryRepository;
            _stateCommandRepository   = stateCommandRepository;
            _cityQueryRepository      = cityQueryRepository;
            _cityCommandRepository    = cityCommandRepository;
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
            var countries      = await _countryQueryRepository.GetByCountryNameAsync(country);
            var matchedCountry = countries.FirstOrDefault(c => Normalize(c.CountryName ?? " ") == normalizedCountry);

            if (matchedCountry is null)
            {
                matchedCountry = new Countries
                {
                    CountryName = country.TrimEnd(),
                    CountryCode = Code(country, 5),
                    IsActive    = Status.Active
                };
                matchedCountry = await _countryCommandRepository.CreateAsync(matchedCountry);
            }

            // --- Step 2: State ---
            var states       = await _stateQueryRepository.GetByStateNameAsync(state);
            var matchedState = states.FirstOrDefault(s =>
                Normalize(s.StateName ?? " ") == normalizedState && s.CountryId == matchedCountry.Id);

            if (matchedState is null)
            {
                matchedState = new States
                {
                    StateCode = Code(state, 5),
                    StateName = state.TrimEnd(),
                    CountryId = matchedCountry.Id,
                    IsActive  = Status.Active
                };
                matchedState = await _stateCommandRepository.CreateAsync(matchedState);
            }

            // --- Step 3: City ---
            var cities      = await _cityQueryRepository.GetByCityNameAsync(city);
            var matchedCity = cities.FirstOrDefault(c =>
                Normalize(c.CityName ?? " ") == normalizedCity && c.StateId == matchedState.Id);

            if (matchedCity is null)
            {
                matchedCity = new Cities
                {
                    CityCode = Code(city, 5),
                    CityName = city.TrimEnd(),
                    StateId  = matchedState.Id,
                    IsActive = Status.Active
                };
                matchedCity = await _cityCommandRepository.CreateAsync(matchedCity);
            }

            return new LocationLookupDto
            {
                CityId    = matchedCity.Id,
                StateId   = matchedState.Id,
                CountryId = matchedCountry.Id
            };
        }
    }
}
