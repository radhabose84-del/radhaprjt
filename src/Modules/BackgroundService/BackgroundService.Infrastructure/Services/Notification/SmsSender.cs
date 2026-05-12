using Microsoft.Extensions.Logging;
using BackgroundService.Infrastructure.Configurations;
using BackgroundService.Application.Interfaces.Notification;
using Contracts.Events.Notifications.Sms;
using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Infrastructure.Repositories.Common;
using Contracts.Interfaces;
using Contracts.Events.Notifications;

namespace BackgroundService.Infrastructure.Services.Notification
{
    public class SmsSender : BaseQueryRepository,ISmsSender
    {
        private readonly SmsSettings _smsSettings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SmsSender> _logger;      
          

        public SmsSender(SmsSettings smsSettings, IHttpClientFactory httpClientFactory, ILogger<SmsSender> logger, IIPAddressService ipAddressService)
        : base(ipAddressService)
        {
            _smsSettings = smsSettings;
            _httpClient = httpClientFactory.CreateClient("SmsClient");
            _logger = logger;         
        }

        public async Task<bool> SendSmsAsync(SendSmsNotificationCommand command)
        {
            try
            {
                if (command.mobileNumbers == null || !command.mobileNumbers.Any())
                {
                    _logger.LogWarning("⚠️ No mobile numbers provided. SMS aborted.");
                    return false;
                }

                foreach (var number in command.mobileNumbers)
                {
                    var success = await SendSmsToSingleAsync(number, command.message);

                    if (!success)
                    {
                        _logger.LogWarning("❌ SMS send failed for number: {Number}", number);
                    }
                    await Task.Delay(50); // optional delay
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send SMS batch.");
                return false;
            }
        }

        public Task<bool> SendSmsAsyncOld(List<string> mobileNumbers, string message)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> SendSmsToSingleAsync(string phoneNumber, string message)
        {
            try
            {
                var url = $"{_smsSettings.BaseUrl}?apikey={_smsSettings.ApiKey}&route={_smsSettings.Route}&sender={_smsSettings.Sender}&number={phoneNumber}&message={Uri.EscapeDataString(message)}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("SMS sending failed to {Phone}: {StatusCode}", phoneNumber, response.StatusCode);
                    return false;
                }
                 Console.WriteLine("from smssender");
                 Console.WriteLine($"🔥 Channels from SQL: PhoneNumber = {phoneNumber}, msg = {message},response= {response},url={url} ");
              
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while sending SMS to {Phone}", phoneNumber);
                return false;
            }
        }
     
    }
}
