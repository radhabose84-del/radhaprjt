using System;
using MassTransit;

namespace Contracts.Events.Notifications.Whatsapp
{
    public class SendWhatsappNotificationCompleted : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}
