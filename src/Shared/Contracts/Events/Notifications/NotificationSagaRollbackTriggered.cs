using System;
using MassTransit;

namespace Contracts.Events.Notifications
{
    public class NotificationSagaRollbackTriggered : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; }
    }
}
