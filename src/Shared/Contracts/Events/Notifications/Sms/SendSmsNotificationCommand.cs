using MediatR;

namespace Contracts.Events.Notifications.Sms
{
    public class SendSmsNotificationCommand : IRequest<bool>
    {
        public List<string> mobileNumbers { get; set; } = default!;
        public string? message { get; set; }
        public int EventRuleId { get; set; }
        public int ChannelId { get; set; }    
        public int EventTypeId { get; set; }    
    }
}