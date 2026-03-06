// BackgroundService.Infrastructure/Services/Notification/WhatsAppSender.cs
using System;
using System.Net.Http;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces.Notification;
using BackgroundService.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Infrastructure.Services.Notification
{
    public class WhatsAppSender : IWhatsAppSender
    {
        private readonly WhatsAppSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<WhatsAppSender> _logger;

        public WhatsAppSender(
            WhatsAppSettings settings,
            IHttpClientFactory httpClientFactory,
            ILogger<WhatsAppSender> logger)
        {
            _settings = settings;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<bool> SendWhatsAppAsync(SendWhatsappNotificationCommand command)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(command.MobileNumber))
                {
                    _logger.LogWarning("No WhatsApp mobile number provided.");
                    return false;
                }

                // ✅ Prefer ApiKey from command (resolved from SQL), fallback to appsettings
                var apiToken = !string.IsNullOrWhiteSpace(command.ApiKey)
                    ? command.ApiKey
                    : _settings.ApiToken;

                if (string.IsNullOrWhiteSpace(apiToken))
                {
                    _logger.LogWarning("WhatsApp ApiToken is empty (both command.ApiKey and settings.ApiToken).");
                    return false;
                }

                var encodedMessage = Uri.EscapeDataString(command.Message ?? string.Empty);
                var encodedNumber = Uri.EscapeDataString(command.MobileNumber);

                // ✅ Encode token too (safe)
                var encodedToken = Uri.EscapeDataString(apiToken);

                // ✅ Use dynamic token in url
                var url = $"{_settings.BaseUrl}?apikey={encodedToken}&gname={encodedNumber}&msg={encodedMessage}";

                // ✅ Do NOT log full token in production logs
                _logger.LogInformation("Sending WhatsApp: To={Number}", command.MobileNumber);

                var resp = await _httpClient.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("WhatsApp send failed to {Number}: {Status}", command.MobileNumber, resp.StatusCode);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send WhatsApp message to {Number}", command.MobileNumber);
                return false;
            }
        }
    }

    public class WhatsAppSettings
    {
        public string BaseUrl { get; set; } = string.Empty;

        // fallback / default token (optional)
        public string ApiToken { get; set; } = string.Empty;
    }
}
