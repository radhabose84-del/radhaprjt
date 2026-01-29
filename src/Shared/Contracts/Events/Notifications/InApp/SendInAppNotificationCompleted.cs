using System;
using MassTransit;

namespace Contracts.Events.Notifications.InApp
{
    public class SendInAppNotificationCompleted : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}
