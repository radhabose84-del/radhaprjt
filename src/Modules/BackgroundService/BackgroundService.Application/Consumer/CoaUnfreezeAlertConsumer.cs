using BackgroundService.Application.Interfaces.IInbox;
using BackgroundService.Application.Interfaces.Notification;
using Contracts.Events.Coa;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumers
{
    /// <summary>
    /// US-GL02-08B (AC2) — sends the unfreeze alert to CFO / FC / Internal Audit. Recipients are
    /// pre-resolved by the Finance handler (no notification-config/SP seeding), so this consumer just
    /// renders a short HTML notice and sends via IEmailSender. Idempotent via the inbox dedup store.
    /// </summary>
    public class CoaUnfreezeAlertConsumer : IConsumer<CoaUnfreezeAlertEvent>
    {
        private readonly IEmailSender _emailSender;
        private readonly IInboxRepository _inbox;
        private readonly ILogger<CoaUnfreezeAlertConsumer> _logger;

        public CoaUnfreezeAlertConsumer(
            IEmailSender emailSender,
            IInboxRepository inbox,
            ILogger<CoaUnfreezeAlertConsumer> logger)
        {
            _emailSender = emailSender;
            _inbox = inbox;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CoaUnfreezeAlertEvent> context)
        {
            var msg = context.Message;
            var messageId = context.MessageId ?? Guid.NewGuid();
            const string consumerName = nameof(CoaUnfreezeAlertConsumer);

            if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
            {
                _logger.LogInformation("Inbox dedup: duplicate skipped. Consumer={Consumer}, MessageId={MessageId}", consumerName, messageId);
                return;
            }

            var recipients = (msg.RecipientEmails ?? new List<string>())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (recipients.Count == 0)
            {
                _logger.LogWarning("CoaUnfreezeAlertConsumer: no recipients for unfreeze request {Id} — nothing to send.", msg.UnfreezeRequestId);
                await _inbox.MarkAsProcessedAsync(consumerName, messageId, msg.CorrelationId, context.CancellationToken);
                return;
            }

            var subject = $"COA unfreeze window opened — request #{msg.UnfreezeRequestId}";
            var header = "<b>Chart of Accounts — Unfreeze Window Opened</b>";
            var body =
                $"A dual-approval unfreeze (request #{msg.UnfreezeRequestId}) has been granted for company {msg.CompanyId}.<br/>" +
                $"Reason: {System.Net.WebUtility.HtmlEncode(msg.Reason ?? string.Empty)}<br/>" +
                $"Approvers: CFO (user {msg.CfoApproverUserId}) and System Admin (user {msg.SysAdminApproverUserId}).<br/>" +
                $"The window auto-re-freezes at {msg.WindowExpiry:u}.";
            var footer = "BSOFT ERP — automated control alert. Do not reply.";

            var sent = await _emailSender.SendEmailAsync(recipients, subject, header, body, footer);
            if (!sent)
                throw new Exception("Email sender returned false for COA unfreeze alert.");

            _logger.LogInformation("CoaUnfreezeAlertConsumer: alert sent to {Count} recipient(s) for unfreeze {Id}.", recipients.Count, msg.UnfreezeRequestId);
            await _inbox.MarkAsProcessedAsync(consumerName, messageId, msg.CorrelationId, context.CancellationToken);
        }
    }
}
