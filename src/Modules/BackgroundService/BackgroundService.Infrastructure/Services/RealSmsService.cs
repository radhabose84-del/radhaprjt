
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Infrastructure.Configurations;
using Contracts.Events.Notifications;


namespace BackgroundService.Infrastructure.Services
{
    public class RealSmsService : ISmsService
    {
        private readonly SmsSettings _smsSettings;
        private readonly IHttpClientFactory _httpClientFactory;
       public RealSmsService(SmsSettings smsSettings, IHttpClientFactory httpClientFactory)
        {          
            _smsSettings = smsSettings;  
            _httpClientFactory = httpClientFactory; // ✅ Ensure HttpClientFactory is injected
        }
        public async Task<bool> SendSmsAsync(SendSmsCommand command)
        {
             var client = _httpClientFactory.CreateClient(); // ✅ Use HttpClientFactory

            string requestUrl = $"{_smsSettings.BaseUrl}?key={_smsSettings.ApiKey}&route={_smsSettings.Route}" +
                                $"&sender={_smsSettings.Sender}&number={command.to}&sms={Uri.EscapeDataString(command.message)}" +
                                $" If you didn't request this,please contact us at {_smsSettings.adminMailId} -BASML"+
                                $"&templateid={_smsSettings.TemplateId}";

            HttpResponseMessage response = await client.GetAsync(requestUrl);

            return response.IsSuccessStatusCode;
        }
    }
}