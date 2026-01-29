using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Users;
using Contracts.Interfaces.External.IUser;
using Grpc.Core;
using GrpcServices.UserManagement;
using Microsoft.AspNetCore.Http;

namespace BackgroundService.Infrastructure.GrpcClients
{
    public class MenuGrpcClient : IMenuGrpcClient
    {
        private readonly MenuService.MenuServiceClient _menuServiceClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public MenuGrpcClient(MenuService.MenuServiceClient menuServiceClient, IHttpContextAccessor httpContextAccessor)
        {
            _menuServiceClient = menuServiceClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> GetMenuByNameAsync(string MenuName)
        {
            var req = new MenuRequestByName();
            req.MenuName = MenuName;

            // var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();

            // if (string.IsNullOrWhiteSpace(token))
            //     throw new UnauthorizedAccessException("Authorization token not found.");

            // if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            //     token = $"Bearer {token}";

            // var metadata = new Metadata
            // {
            //     { "Authorization", token }
            // };

            // var response = await _menuServiceClient.GetMenuByNameAsync(req, new CallOptions(metadata));
            var response = await _menuServiceClient.GetMenuByNameAsync(req);

            return response.Id;
        }

        public async Task<List<Contracts.Dtos.Users.MenuDto>> GetMenuIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var req = new MenuRequest();
               req.Ids.AddRange(ids?.Distinct() ?? Enumerable.Empty<int>());

            // var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();

            // if (string.IsNullOrWhiteSpace(token))
            //     throw new UnauthorizedAccessException("Authorization token not found.");

            // if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            //     token = $"Bearer {token}";

            // var metadata = new Metadata
            // {
            //     { "Authorization", token }
            // };

            // var response = await _menuServiceClient.GetMenuByIdsAsync(req, new CallOptions(metadata));
            var response = await _menuServiceClient.GetMenuByIdsAsync(req);

            return response.Menus.Select(u => new Contracts.Dtos.Users.MenuDto
            {
                Id = u.Id,
                MenuName = u.MenuName
            }).ToList();
        }
    }
}