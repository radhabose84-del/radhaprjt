using PartyManagement.Application.Common.Interfaces.IOutbox;
using PartyManagement.Domain.Entities.Outbox;
using PartyManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PartyManagement.Infrastructure.Repositories.Outbox
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OutboxRepository> _logger;

        public OutboxRepository(ApplicationDbContext context, ILogger<OutboxRepository> logger)
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
        }

        public async Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 100, CancellationToken cancellationToken = default)
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
            var message = await _context.OutboxMessages.FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
            if (message != null)
            {
                message.Status = OutboxMessageStatus.Published;
                message.PublishedAt = DateTimeOffset.UtcNow;
                message.LastError = null;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task MarkAsFailedAsync(long messageId, string errorMessage, CancellationToken cancellationToken = default)
        {
            var message = await _context.OutboxMessages.FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
            if (message != null)
            {
                message.RetryCount++;
                message.LastError = errorMessage.Length > 2000 ? errorMessage[..2000] : errorMessage;
                message.NextRetryAt = DateTimeOffset.UtcNow.AddSeconds(Math.Pow(2, message.RetryCount));
                if (message.RetryCount >= message.MaxRetries)
                    message.Status = OutboxMessageStatus.Failed;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteOldMessagesAsync(int daysToKeep = 7, CancellationToken cancellationToken = default)
        {
            var cutoff = DateTimeOffset.UtcNow.AddDays(-daysToKeep);
            var deleted = await _context.OutboxMessages
                .Where(m => m.Status == OutboxMessageStatus.Published && m.PublishedAt < cutoff)
                .ExecuteDeleteAsync(cancellationToken);

            if (deleted > 0)
                _logger.LogInformation("Deleted {Count} old Party outbox messages", deleted);
        }

        public async Task<OutboxMessage?> GetByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default)
        {
            return await _context.OutboxMessages.FirstOrDefaultAsync(m => m.CorrelationId == correlationId, cancellationToken);
        }
    }
}
