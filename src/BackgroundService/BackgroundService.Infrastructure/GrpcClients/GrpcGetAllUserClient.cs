using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Users;
using Contracts.Interfaces.External.IUser;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcServices.UserManagement;
using Microsoft.AspNetCore.Http;

namespace BackgroundService.Infrastructure.GrpcClients
{
    public class GrpcGetAllUserClient : IUsersAllGrpcClient
    {
        private readonly GetAllUsersJobService.GetAllUsersJobServiceClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public GrpcGetAllUserClient(GetAllUsersJobService.GetAllUsersJobServiceClient client, IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<List<Contracts.Dtos.Users.UsersAllDto>> GetUserAllAsync()
        {
            var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
           
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("No Authorization token found in the current context.");
            }

            if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = $"Bearer {token}";
            }

            var metadata = new Metadata
            {
                { "Authorization", token }
            };

            var callOptions = new CallOptions(metadata);

            var response = await _client.GetUserAllAsync(new Empty(), callOptions);

            return response.Users.Select(proto => new Contracts.Dtos.Users.UsersAllDto
            {
                UserId = proto.UserId,
                UserName = proto.UserName
            }).ToList();
        }
    }
}