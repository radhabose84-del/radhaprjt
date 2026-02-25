using MassTransit;

namespace Contracts.Events.Notifications.Sms
{
    public class SendSmsNotificationCompleted : CorrelatedBy<Guid>
    {        public Guid CorrelationId { get; set; }
    }
}
