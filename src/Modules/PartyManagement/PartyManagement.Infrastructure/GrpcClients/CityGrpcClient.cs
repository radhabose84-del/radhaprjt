using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Interfaces.External.IUser;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcServices.UserManagement;
using Microsoft.AspNetCore.Http;

namespace PartyManagement.Infrastructure.GrpcClients
{
    public class CityGrpcClient : ICityGrpcClient
    {

        private readonly CityService.CityServiceClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CityGrpcClient(CityService.CityServiceClient client, IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Contracts.Dtos.Users.CityDto>> GetAllCityAsync()
        {
            var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("Authorization token not found.");

            if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = $"Bearer {token}";

            var metadata = new Metadata
            {
                { "Authorization", token }
            };

            var response = await _client.GetAllCityAsync(new Empty(), new CallOptions(metadata));

            return response.Cities.Select(u => new Contracts.Dtos.Users.CityDto
            {
                CityId = u.CityId,
                CityName = u.CityName,
                CityCode = u.CityCode,
                StateId = u.StateId
            }).ToList();
        }
    }
}