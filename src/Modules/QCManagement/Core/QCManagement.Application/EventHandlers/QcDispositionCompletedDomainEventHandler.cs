using MediatR;
using QCManagement.Domain.Events;

namespace QCManagement.Application.EventHandlers
{
    /// <summary>
    /// No-op stub for SCRUM-1667. Inventory movement / putaway-rule creation is owned by
    /// PurchaseManagement / InventoryManagement and will subscribe to this event in a later ticket.
    /// </summary>
    public class QcDispositionCompletedDomainEventHandler : INotificationHandler<QcDispositionCompletedDomainEvent>
    {
        public Task Handle(QcDispositionCompletedDomainEvent notification, CancellationToken cancellationToken)
        {
            // TODO (future ticket): create GrnPutAwayRule rows + StockLedger entries based on disposition.
            return Task.CompletedTask;
        }
    }
}
