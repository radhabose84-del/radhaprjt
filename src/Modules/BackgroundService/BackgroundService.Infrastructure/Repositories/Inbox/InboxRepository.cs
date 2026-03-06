using BackgroundService.Application.Interfaces.IInbox;
using BackgroundService.Infrastructure.Data.Notification;
using BackgroundService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Infrastructure.Repositories.Inbox
{
    /// <summary>
    /// SQL Server-backed inbox repository using NotificationDbContext.
    /// The unique index on (ConsumerName, MessageId) enforces dedup at the database level.
    /// </summary>
    internal sealed class InboxRepository : IInboxRepository
    {
        private readonly NotificationDbContext _context;
        private readonly ILogger<InboxRepository> _logger;

        public InboxRepository(NotificationDbContext context, ILogger<InboxRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> IsAlreadyProcessedAsync(
            string consumerName,
            Guid messageId,
            CancellationToken cancellationToken = default)
        {
            return await _context.InboxMessages
                .AnyAsync(m => m.ConsumerName == consumerName && m.MessageId == messageId, cancellationToken);
        }

        public async Task MarkAsProcessedAsync(
            string consumerName,
            Guid messageId,
            Guid? correlationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entry = new InboxMessage
                {
                    ConsumerName = consumerName,
                    MessageId = messageId,
                    CorrelationId = correlationId,
                    ProcessedAt = DateTimeOffset.UtcNow
                };

                _context.InboxMessages.Add(entry);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UQ_InboxMessages_Consumer_MessageId") == true
                                             || ex.InnerException?.Message.Contains("duplicate key") == true)
            {
                // Race condition: another instance already inserted this record — safe to ignore.
                _logger.LogWarning(
                    "Inbox dedup: concurrent insert detected for Consumer={ConsumerName}, MessageId={MessageId}. Ignoring.",
                    consumerName, messageId);
            }
        }
    }
}
