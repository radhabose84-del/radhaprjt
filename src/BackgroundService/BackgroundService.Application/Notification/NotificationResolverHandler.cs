using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces.Notification;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Notification
{
    public class NotificationResolverHandler : INotificationResolverHandler
    {
        private readonly INotificationUserResolver _resolver;
        private readonly ILogger<NotificationResolverHandler> _logger;

        public NotificationResolverHandler(
            INotificationUserResolver resolver,
            ILogger<NotificationResolverHandler> logger)
        {
            _resolver = resolver;
            _logger = logger;
        }

        public async Task<List<string>> ResolveNotificationChannelsAsync(
            int unitId, string module, int eventTypeId,
            string email, string ccMail, string mobile)
        {
            var targets = await _resolver.GetNotificationTargetsAsync(
                unitId, module, eventTypeId,
                email ?? string.Empty,
                ccMail ?? string.Empty,
                mobile ?? string.Empty);

            if (targets == null || targets.Count == 0)
            {
                _logger.LogInformation(
                    "No notification targets resolved for Unit={UnitId}, Module={Module}, EventType={EventTypeId}",
                    unitId, module, eventTypeId);

                return new List<string>();
            }

            var discovered = targets
                .Select(t => t.ChannelName?.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var normalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var ch in discovered)
            {
                if (ch.Equals("Email", StringComparison.OrdinalIgnoreCase))
                    normalized.Add("Email");
                else if (ch.Equals("SMS", StringComparison.OrdinalIgnoreCase) ||
                         ch.Equals("Sms", StringComparison.OrdinalIgnoreCase))
                    normalized.Add("SMS");
                else if (ch.Equals("InApp", StringComparison.OrdinalIgnoreCase) ||
                         ch.Equals("In-App", StringComparison.OrdinalIgnoreCase))
                    normalized.Add("InApp");
                else if (ch.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase) ||
                         ch.Equals("Whatsapp", StringComparison.OrdinalIgnoreCase) ||
                         ch.Equals("WA", StringComparison.OrdinalIgnoreCase))
                    normalized.Add("WhatsApp");
                else
                    _logger.LogWarning("Unknown ChannelName from SQL: {ChannelName}", ch);
            }

            // Fallback: if no explicit SMS channel but mobile numbers exist, add "SMS"
            var hasMobiles = targets.Any(t => !string.IsNullOrWhiteSpace(t.TargetMobileNumbers));
            if (hasMobiles && !normalized.Contains("SMS"))
            {
                _logger.LogInformation(
                    "No explicit SMS channel row, but mobile numbers found. Adding 'SMS' as fallback channel. Unit={UnitId}, Module={Module}, EventType={EventTypeId}",
                    unitId, module, eventTypeId);

                normalized.Add("SMS");
            }

            var result = normalized.ToList();

            _logger.LogInformation(
                "Resolved channels from SQL for Unit={UnitId}, Module={Module}, EventTypeId={EventTypeId} => [{Channels}]",
                unitId, module, eventTypeId, string.Join(", ", result));

            return result;
        }

        public async Task<(List<string> To, List<string> Cc, List<string> Bcc,
                           List<string> Sms, List<int> InApp,
                           string Subject, string Header, string Body, string Footer,
                           string Lang, int? EventTypeId, int? EventRuleId, int? ChannelId,
                           int TemplateId, bool IsTable, string? ApiToken)>
            ResolveNotificationTemplatesAsync(
                int unitId, string module, int eventTypeId,
                string email, string ccMail, string mobile,
                string p1, string p2, DateTimeOffset p3,
                string? p4, string? p5, string? p6, string? p7, string? p8, string? p9, string? p10,
                IReadOnlyCollection<(string FileName, string ContentType, byte[] Data)>? attachments = null)
        {
            var targets = await _resolver.GetNotificationTargetsAsync(
                unitId, module, eventTypeId,
                email ?? string.Empty,
                ccMail ?? string.Empty,
                mobile ?? string.Empty);

            // ✅ SAFETY: if SP returns nothing, avoid null refs and return empty defaults
            if (targets == null || targets.Count == 0)
            {
                _logger.LogInformation(
                    "No templates/targets resolved for Unit={UnitId}, Module={Module}, EventTypeId={EventTypeId}",
                    unitId, module, eventTypeId);

                return (new List<string>(), new List<string>(), new List<string>(),
                        new List<string>(), new List<int>(),
                        string.Empty, string.Empty, string.Empty, string.Empty,
                        "en", null, null, null,
                        0, false, null);
            }

            static IEnumerable<string> Split(string? s) =>
                string.IsNullOrWhiteSpace(s)
                    ? Array.Empty<string>()
                    : s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // EMAIL rows
            var emailTargets = targets
                .Where(t => string.Equals(t.ChannelName, "Email", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var to = emailTargets
                .SelectMany(t => Split(t.TargetEmailIds))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var cc = emailTargets
                .SelectMany(t => Split(t.TargetCcEmails))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var bcc = emailTargets
                .SelectMany(t => Split(t.TargetBccEmails))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // SMS rows (also reused by WhatsApp consumer)
            var smsRows = targets
                .Where(t => string.Equals(t.ChannelName, "SMS", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(t.ChannelName, "Sms", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // 🔁 Fallback: if no explicit SMS rows, use any row that has TargetMobileNumbers filled
            if (!smsRows.Any())
            {
                smsRows = targets
                    .Where(t => !string.IsNullOrWhiteSpace(t.TargetMobileNumbers))
                    .ToList();
            }

            var sms = smsRows
                .SelectMany(t => Split(t.TargetMobileNumbers))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // InApp rows
            var inApp = targets
                .Where(t => string.Equals(t.ChannelName, "InApp", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(t.ChannelName, "In-App", StringComparison.OrdinalIgnoreCase))
                .SelectMany(t => Split(t.TargetUserIds))
                .Select(s => int.TryParse(s, out var id) ? id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            // ✅ Template row: prefer Email row for subject/body/etc.
            var templateRow = emailTargets.FirstOrDefault() ?? targets.FirstOrDefault();

            // ✅ ApiToken must be chosen by channel: prefer WhatsApp token first, else SMS, else templateRow
            var waRow = targets.FirstOrDefault(t =>
                string.Equals(t.ChannelName, "WhatsApp", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.ChannelName, "Whatsapp", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.ChannelName, "WA", StringComparison.OrdinalIgnoreCase));

            var smsRow = targets.FirstOrDefault(t =>
                string.Equals(t.ChannelName, "SMS", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.ChannelName, "Sms", StringComparison.OrdinalIgnoreCase));

            var apiToken = waRow?.ApiToken
                        ?? smsRow?.ApiToken
                        ?? templateRow?.ApiToken;

            var subject = templateRow?.SubjectTemplate ?? string.Empty;
            var header = templateRow?.HeaderTemplate ?? string.Empty;
            var body = templateRow?.BodyTemplate ?? string.Empty;
            var footer = templateRow?.FooterTemplate ?? string.Empty;
            var lang = templateRow?.LanguageCode ?? "en";

            var evtId = templateRow?.EventTypeId;
            var ruleId = templateRow?.EventRuleId;
            var channelId = templateRow?.ChannelId;
            var templateId = templateRow?.TemplateId ?? 0;
            var isTable = templateRow?.IsTable ?? false;

            return (to, cc, bcc, sms, inApp,
                    subject, header, body, footer,
                    lang, evtId, ruleId, channelId,
                    templateId, isTable, apiToken);
        }
    }
}
