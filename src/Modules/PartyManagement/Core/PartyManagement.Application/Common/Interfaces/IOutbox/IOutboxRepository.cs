using PartyManagement.Domain.Entities.Outbox;

namespace PartyManagement.Application.Common.Interfaces.IOutbox
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
        Task AddWithoutSaveAsync(OutboxMessage message, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 100, CancellationToken cancellationToken = default);
        Task MarkAsPublishedAsync(long messageId, CancellationToken cancellationToken = default);
        Task MarkAsFailedAsync(long messageId, string errorMessage, CancellationToken cancellationToken = default);
        Task DeleteOldMessagesAsync(int daysToKeep = 7, CancellationToken cancellationToken = default);
        Task<OutboxMessage?> GetByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default);
    }
}
