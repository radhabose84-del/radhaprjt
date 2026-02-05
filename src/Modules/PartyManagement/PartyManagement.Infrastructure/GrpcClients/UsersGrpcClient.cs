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
    public class UsersGrpcClient : IUsersAllGrpcClient
    {
        private readonly GetAllUsersJobService.GetAllUsersJobServiceClient _getAllUsersJobServiceClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UsersGrpcClient(GetAllUsersJobService.GetAllUsersJobServiceClient getAllUsersJobServiceClient, IHttpContextAccessor httpContextAccessor)
        {
            _getAllUsersJobServiceClient = getAllUsersJobServiceClient;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<List<Contracts.Dtos.Users.UsersAllDto>> GetUserAllAsync()
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
            

            var response = await _getAllUsersJobServiceClient.GetUserAllAsync(new Empty(), new CallOptions(metadata));

            return response.Users.Select(u => new Contracts.Dtos.Users.UsersAllDto
            {
                UserId = u.UserId,
                UserName = u.UserName
            }).ToList();
        }
    }
}