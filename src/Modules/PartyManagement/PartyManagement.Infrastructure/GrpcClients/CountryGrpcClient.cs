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
    public class CountryGrpcClient : ICountryGrpcClient
    {
        private readonly CountryService.CountryServiceClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CountryGrpcClient(CountryService.CountryServiceClient client, IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Contracts.Dtos.Users.CountryDto>> GetAllCountryAsync()
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

            var response = await _client.GetAllCountryAsync(new Empty(), new CallOptions(metadata));

            return response.Countries.Select(u => new Contracts.Dtos.Users.CountryDto
            {
                CountryId = u.CountryId,
                CountryName = u.CountryName,
                CountryCode = u.CountryCode
            }).ToList();
        }
    }
}