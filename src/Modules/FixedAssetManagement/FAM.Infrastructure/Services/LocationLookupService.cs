// using Contracts.Interfaces.External.IUser;
// using FAM.Application.Common.Interfaces;

// namespace FAM.Infrastructure.Services
// {
//     public class LocationLookupService : ILocationLookupService
//     {
//         private readonly ICountryGrpcClient _countryClient;
//         private readonly ICityGrpcClient _cityClient;
//         private readonly IStatesGrpcClient _stateClient;

//         public LocationLookupService(ICountryGrpcClient countryClient, ICityGrpcClient cityClient, IStatesGrpcClient stateClient)
//         {
//             _countryClient = countryClient;
//             _cityClient = cityClient;
//             _stateClient = stateClient;
//         }

//         public async Task<Dictionary<int, string>> GetCountryLookupAsync()
//         {
//             var countries = await _countryClient.GetAllCountryAsync();
//             return countries.ToDictionary(c => c.CountryId, c => c.CountryName);
//         }

//         public async Task<Dictionary<int, string>> GetStateLookupAsync()
//         {
//             var states = await _stateClient.GetAllStateAsync();
//             return states.ToDictionary(s => s.StateId, s => s.StateName);
//         }

//         public async Task<Dictionary<int, string>> GetCityLookupAsync()
//         {
//             var cities = await _cityClient.GetAllCityAsync();
//             return cities.ToDictionary(c => c.CityId, c => c.CityName);
//         }
//     }
// }