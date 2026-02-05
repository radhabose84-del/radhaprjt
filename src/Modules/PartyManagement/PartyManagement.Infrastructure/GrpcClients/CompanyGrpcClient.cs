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
    public class CompanyGrpcClient : ICompanyGrpcClient
    {
        private readonly CompanyService.CompanyServiceClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CompanyGrpcClient(CompanyService.CompanyServiceClient client, IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor; // ✅ fixed
        }
         public async Task<List<Contracts.Dtos.Users.CompanyDto>> GetAllCompanyAsync()
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

            var response = await _client.GetAllCompanyAsync(new Empty(), new CallOptions(metadata));

            return response.Companies.Select(u => new Contracts.Dtos.Users.CompanyDto
            {
                CompanyId = u.CompanyId,
                CompanyName = u.CompanyName,
                LegalName=u.LegalName,
                GstNumber=u.GstNumber,
                TinNumber=u.TinNumber,
                TanNumber=u.TanNumber,
                CstNumber=u.CstNumber,
                EntityId=u.EntityId
            }).ToList();
        }
    }
}