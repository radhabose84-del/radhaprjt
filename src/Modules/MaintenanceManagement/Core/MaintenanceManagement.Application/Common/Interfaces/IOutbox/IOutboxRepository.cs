using MaintenanceManagement.Domain.Entities.Outbox;

namespace MaintenanceManagement.Application.Common.Interfaces.IOutbox
{
    /// <summary>
    /// Repository for managing outbox messages.
    /// Supports atomic operations within the same database transaction.
    /// </summary>
    public interface IOutboxRepository
    {
        /// <summary>
        /// Adds an outbox message to the current DbContext (participates in transaction)
        /// </summary>
        Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets pending messages ready for publishing (Status=Pending, NextRetryAt <= now)
        /// </summary>
        Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
            int batchSize = 100,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks a message as successfully published
        /// </summary>
        Task MarkAsPublishedAsync(long messageId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks a message as failed with error details and schedules retry
        /// </summary>
        Task MarkAsFailedAsync(
            long messageId,
            string errorMessage,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes old published messages (cleanup)
        /// </summary>
        Task DeleteOldMessagesAsync(
            int daysToKeep = 7,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets message by correlation ID for tracing
        /// </summary>
        Task<OutboxMessage?> GetByCorrelationIdAsync(
            Guid correlationId,
            CancellationToken cancellationToken = default);
    }
}
