using MaintenanceManagement.Application.Common.Interfaces.IOutbox;
using MaintenanceManagement.Domain.Entities.Outbox;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.Infrastructure.Repositories.Outbox
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OutboxRepository> _logger;

        public OutboxRepository(
            ApplicationDbContext context,
            ILogger<OutboxRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            await _context.OutboxMessages.AddAsync(message, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task AddWithoutSaveAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            await _context.OutboxMessages.AddAsync(message, cancellationToken);
            // Do NOT save — caller is responsible for transaction management.
            // This allows the outbox message to participate in the same transaction as other operations.
        }

        public async Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
            int batchSize = 100,
            CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;

            return await _context.OutboxMessages
                .Where(m => m.Status == OutboxMessageStatus.Pending)
                .Where(m => m.NextRetryAt == null || m.NextRetryAt <= now)
                .Where(m => m.RetryCount < m.MaxRetries)
                .OrderBy(m => m.CreatedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        public async Task MarkAsPublishedAsync(long messageId, CancellationToken cancellationToken = default)
        {
            var message = await _context.OutboxMessages
                .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

            if (message != null)
            {
                message.Status = OutboxMessageStatus.Published;
                message.PublishedAt = DateTimeOffset.UtcNow;
                message.LastError = null;

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogDebug(
                    "Outbox message {MessageId} marked as published. CorrelationId: {CorrelationId}",
                    messageId, message.CorrelationId);
            }
        }

        public async Task MarkAsFailedAsync(
            long messageId,
            string errorMessage,
            CancellationToken cancellationToken = default)
        {
            var message = await _context.OutboxMessages
                .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

            if (message != null)
            {
                message.RetryCount++;
                message.LastError = errorMessage.Length > 2000
                    ? errorMessage[..2000]
                    : errorMessage;

                // Exponential backoff: 1s, 2s, 4s, 8s, 16s...
                var delaySeconds = Math.Pow(2, message.RetryCount);
                message.NextRetryAt = DateTimeOffset.UtcNow.AddSeconds(delaySeconds);

                if (message.RetryCount >= message.MaxRetries)
                {
                    message.Status = OutboxMessageStatus.Failed;
                    _logger.LogError(
                        "Outbox message {MessageId} permanently failed after {RetryCount} attempts. CorrelationId: {CorrelationId}, Error: {Error}",
                        messageId, message.RetryCount, message.CorrelationId, errorMessage);
                }
                else
                {
                    _logger.LogWarning(
                        "Outbox message {MessageId} failed (attempt {RetryCount}/{MaxRetries}). Next retry at {NextRetryAt}. Error: {Error}",
                        messageId, message.RetryCount, message.MaxRetries, message.NextRetryAt, errorMessage);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteOldMessagesAsync(
            int daysToKeep = 7,
            CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTimeOffset.UtcNow.AddDays(-daysToKeep);

            var deletedCount = await _context.OutboxMessages
                .Where(m => m.Status == OutboxMessageStatus.Published)
                .Where(m => m.PublishedAt < cutoffDate)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount > 0)
            {
                _logger.LogInformation(
                    "Deleted {Count} old outbox messages (older than {Days} days)",
                    deletedCount, daysToKeep);
            }
        }

        public async Task<OutboxMessage?> GetByCorrelationIdAsync(
            Guid correlationId,
            CancellationToken cancellationToken = default)
        {
            return await _context.OutboxMessages
                .FirstOrDefaultAsync(m => m.CorrelationId == correlationId, cancellationToken);
        }
    }
}
