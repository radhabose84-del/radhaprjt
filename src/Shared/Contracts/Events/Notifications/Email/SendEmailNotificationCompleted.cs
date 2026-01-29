using System;
using MassTransit;

namespace Contracts.Events.Notifications.Email
{
    public class SendEmailNotificationCompleted : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}
