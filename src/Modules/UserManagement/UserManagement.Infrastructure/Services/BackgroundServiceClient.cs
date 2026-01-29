
using Core.Application.Common.Interfaces;
using Contracts.Events.Notifications;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Infrastructure.Services
{
    public class BackgroundServiceClient  : IBackgroundServiceClient
    {
    private readonly IHttpClientFactory _httpClientFactory;
    // private readonly IHttpContextAccessor _httpContextAccessor;

        public BackgroundServiceClient(IHttpClientFactory httpClientFactory
        // , IHttpContextAccessor httpContextAccessor
        )
        {
            _httpClientFactory = httpClientFactory;
            // _httpContextAccessor = httpContextAccessor;
        }

        public async Task UserUnlock(string userName, int delayInMinutes)
        {
            //  var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();

            // if (string.IsNullOrEmpty(token))
            // {
            //     throw new Exception("No Authorization token found in the current context.");
            // }
            // //  ✅ Ensure it has "Bearer " prefix
            // if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            // {

            //     token = $"Bearer {token}";
            // }


             var client = _httpClientFactory.CreateClient("BackgroundServiceClient");

            //  client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

            var removeCodeCommand = new ScheduleRemoveCodeCommand
            {
                UserName = userName,
                DelayInMinutes = delayInMinutes
            };
            var response = await client.PostAsJsonAsync("/api/userhangfirejobs/user-unlock", removeCodeCommand);
            response.EnsureSuccessStatusCode();
        }

        public async Task ScheduleVerificationCodeCleanupAsync(string userName, int delayInMinutes)
        {
            // var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();

            // if (string.IsNullOrEmpty(token))
            // {
            //     throw new Exception("No Authorization token found in the current context.");
            // }
            // //  ✅ Ensure it has "Bearer " prefix
            // if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            // {

            //     token = $"Bearer {token}";
            // }
            // var client = _httpClientFactory.CreateClient("BackgroundServiceClient");

            // client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
             var client = _httpClientFactory.CreateClient("BackgroundServiceClient");

            var removeCodeCommand = new ScheduleRemoveCodeCommand
            {
                UserName = userName,
                DelayInMinutes = delayInMinutes
            };
            var response = await client.PostAsJsonAsync("/api/userhangfirejobs/user-verification-code-removal", removeCodeCommand);
            response.EnsureSuccessStatusCode();
        }      
    }   

}