
using System.Net;
using System.Net.Mail;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Infrastructure.Configurations;
using Contracts.Events.Notifications;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BackgroundService.Infrastructure.Services
{
    public class RealEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<RealEmailService> _logger;

        public RealEmailService(EmailSettings emailSettings, ILogger<RealEmailService> logger)
        {
            _emailSettings = emailSettings;
             _logger = logger;
        }

        public async Task<bool> SendEmailAsync(SendEmailCommand command)
        {
            var providerKey = string.IsNullOrEmpty(command.Provider) ? "Gmail" : command.Provider;
            if (!_emailSettings.Providers.TryGetValue(providerKey, out var provider))
            {
                Log.Information("Provider '{ProviderKey}' not found in EmailSettings.", providerKey);
                return false;
            }
            try
            {
                _logger.LogInformation(" from RealEmailService");    

                var smtpClient = new SmtpClient(provider.Host)
                {
                    Port = provider.Port,
                    Credentials = new NetworkCredential(provider.UserName, provider.Password),
                    EnableSsl = provider.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(provider.UserName ?? string.Empty),
                    Subject = command.Subject,
                    Body = command.HtmlContent,
                    IsBodyHtml = true
                };

                mail.To.Add(command.ToEmail!);

                await smtpClient.SendMailAsync(mail);
                Log.Information("Email sent to {ToEmail} via {ProviderKey}", command.ToEmail, providerKey);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send email: {ErrorMessage}", ex.Message);
                return false;
            }
        }
    }
}
