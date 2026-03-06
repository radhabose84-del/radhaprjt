using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces;
using Hangfire;

namespace BackgroundService.Infrastructure.Services
{
    [Queue("user_unlock_queue")]
    public class UserUnlockService : IUserUnlockService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserUnlockService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task UnlockUser(string userName)
        {
            var client = _httpClientFactory.CreateClient("UserManagementClient");
            await client.PostAsJsonAsync("api/auth/unlock", new { Username = userName });
        }
    }
}