using BackgroundService.Application.Interfaces;
using BackgroundService.Application.Interfaces.Notification;
using Contracts.Interfaces;
using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Infrastructure.Configurations;
using Contracts.Events.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BackgroundService.Infrastructure.Services.Notification
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailSender> _logger;
        public EmailSender(EmailSettings emailSettings, ILogger<EmailSender> logger)
        {
            _emailSettings = emailSettings;
            _logger = logger;
        }
        
        public async Task<bool> SendEmailAsync(List<string> emails, string subject, string header,string message, string footer, List<string>? CcEmails = null, List<string>? BccEmails = null, int channelId = 0, int eventTypeId = 0, int eventRuleId = 0,
        IEnumerable<(string FileName, string ContentType, byte[] Data)>? attachments = null)
        {
            try
            {                   
                if (emails == null || !emails.Any())
                {
                    _logger.LogWarning(" No recipients provided. Email sending aborted.");
                    return false;
                }

                var providerKey = emails.Any(e => e.Contains("zimbra", StringComparison.OrdinalIgnoreCase))
                    ? "Zimbra"
                    : "Gmail";

                if (!_emailSettings.Providers.TryGetValue(providerKey, out var provider))
                {
                    _logger.LogError("Email provider '{ProviderKey}' not found in EmailSettings.", providerKey);
                    return false;
                }

                using var smtpClient = new SmtpClient(provider.Host, provider.Port)
                {
                    Credentials = new NetworkCredential(provider.UserName, provider.Password),
                    EnableSsl = provider.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                using var mailMessage = new MailMessage
                {
                        From = new MailAddress(provider.UserName),
                        Subject = subject,
                        Body = $"{header}<br/><br/>{message}<br/><br/>{footer}",
                        IsBodyHtml = true
                };

                foreach (var toEmail in emails.Where(e => !string.IsNullOrWhiteSpace(e)))
                {
                    mailMessage.To.Add(toEmail.Trim());
                }

                if (!mailMessage.To.Any())
                {
                    _logger.LogWarning("⚠️ After filtering, no valid 'To' addresses found. Skipping email.");
                    return false;
                }

                if (CcEmails != null)
                {
                    foreach (var cc in CcEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
                        mailMessage.CC.Add(cc.Trim());
                }

                if (BccEmails != null)
                {
                    foreach (var bcc in BccEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
                        mailMessage.Bcc.Add(bcc.Trim());
                }
                    // ✅ Attach files if provided
                if (attachments != null)
                {
                    foreach (var (FileName, ContentType, Data) in attachments)
                    {
                        if (Data == null || Data.Length == 0) continue;

                        var ms = new MemoryStream(Data); // will be disposed by Attachment
                        var safeName = string.IsNullOrWhiteSpace(FileName) ? "attachment.bin" : FileName;
                        var ct = string.IsNullOrWhiteSpace(ContentType) ? "application/octet-stream" : ContentType;

                        var attachment = new Attachment(ms, safeName, ct);
                        mailMessage.Attachments.Add(attachment);
                    }
                }

                _logger.LogInformation(
                    "Sending email: To={To} CC={Cc} BCC={Bcc} Subject='{Subject}' Attachments={AttachmentCount} (Channel={ChannelId}, Rule={RuleId}, Type={TypeId})",
                    string.Join(",", mailMessage.To.Select(t => t.Address)),
                    string.Join(",", mailMessage.CC.Select(t => t.Address)),
                    string.Join(",", mailMessage.Bcc.Select(t => t.Address)),
                    mailMessage.Subject,
                    mailMessage.Attachments.Count,
                    channelId, eventRuleId, eventTypeId);

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (SmtpException smtpex)
            {
                _logger.LogError(smtpex, "SMTP failure while sending email to: {Recipients}",
                    string.Join(",", emails ?? new List<string>()));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to: {Recipients}",
                    string.Join(",", emails ?? new List<string>()));
                return false;
            }
        }        
    }
}
