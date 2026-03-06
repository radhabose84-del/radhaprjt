using Contracts.Events.Notifications;
using BackgroundService.Application.Interfaces;
using BackgroundService.Application;
using Hangfire;
using System.Net.Http.Json;

namespace BackgroundService.Infrastructure.Services
{
    [Queue("forgot_password_queue")]
    public class VerificationCodeCleanupService : IVerificationCodeCleanupService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public VerificationCodeCleanupService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task RemoveVerificationCode(string userName, int delayInMinutes)
        {
            var client = _httpClientFactory.CreateClient("UserManagementClient");
            await client.PostAsJsonAsync("api/User/verfication-code-remove", new { UserName = userName });
        }
    }
}       