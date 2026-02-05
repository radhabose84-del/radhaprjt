using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Party;
using Contracts.Interfaces.External.IParty;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Shared.Grpc;

namespace PartyManagement.Infrastructure.GrpcClients
{
    public class GrpcLocationClient : ILocationGrpcClient
    {
        private readonly LocationService.LocationServiceClient _client; 
        private readonly IHttpContextAccessor _httpContextAccessor;
        public GrpcLocationClient(LocationService.LocationServiceClient client, IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
        }

        public async  Task<LocationDto?> GetOrCreateLocationAsync(string city, string state, string country)
        {
            var request = new LocationRequest
            {
                City = city,
                State = state,
                Country = country
            };

            // ✅ Get token from current HTTP Context
            var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("No Authorization token found in the current context.");
            }
            //  ✅ Ensure it has "Bearer " prefix
            if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {

                token = $"Bearer {token}";
            }

            var metadata = new Metadata
                {
                    { "Authorization", token }
                };
            //  ✅ Attach Authorization header
            var callOptions = new CallOptions(metadata);


            var response = await _client.GetOrCreateLocationAsync(request, callOptions);

            return new LocationDto
            {
                CityId = response.CityId,
                StateId = response.StateId,
                CountryId = response.CountryId
            };
        }
    }
}