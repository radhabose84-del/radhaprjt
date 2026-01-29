    using System;
    using MassTransit;
    namespace Contracts.Events.Notifications.Email
    {
        public class SendEmailNotificationFailed : CorrelatedBy<Guid>
        {
            public Guid CorrelationId { get; set; }
            public string Reason { get; set; } = string.Empty;
        }
    }
