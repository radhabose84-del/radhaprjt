namespace BackgroundService.Application.Interfaces.IInbox
{
    /// <summary>
    /// Inbox/dedup repository — prevents duplicate message processing on MassTransit redelivery.
    /// Each consumer checks this before processing and records completion after success.
    /// </summary>
    public interface IInboxRepository
    {
        /// <summary>
        /// Returns true if this message has already been successfully processed by the given consumer.
        /// </summary>
        Task<bool> IsAlreadyProcessedAsync(
            string consumerName,
            Guid messageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Records that the message was successfully processed.
        /// Call this AFTER the consumer's business logic completes successfully.
        /// </summary>
        Task MarkAsProcessedAsync(
            string consumerName,
            Guid messageId,
            Guid? correlationId,
            CancellationToken cancellationToken = default);
    }
}
