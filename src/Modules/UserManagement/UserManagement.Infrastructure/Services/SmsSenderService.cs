
using System.Net.Http.Json;
using UserManagement.Application.Common.Interfaces;
using Contracts.Events.Notifications;


namespace UserManagement.Infrastructure.Services
{
    public class SmsSenderService : ISmsService
    {        
        private readonly IHttpClientFactory _httpClientFactory;
        // private readonly IHttpContextAccessor _httpContextAccessor;

        public SmsSenderService(IHttpClientFactory httpClientFactory
        // , IHttpContextAccessor httpContextAccessor
        )
        {
            _httpClientFactory = httpClientFactory;
            // _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> SendSmsAsync(SendSmsCommand command)
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
            var client = _httpClientFactory.CreateClient("BackgroundServiceClient");
            // client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
            var response = await client.PostAsJsonAsync("api/sms/send", command);
            return response.IsSuccessStatusCode;
        }
    }
}
