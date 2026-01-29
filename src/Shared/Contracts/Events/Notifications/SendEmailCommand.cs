

using MediatR;

namespace Contracts.Events.Notifications
{
    public class SendEmailCommand : IRequest<bool>
    {
        public string? ToEmail { get; set; }
        public string? Subject { get; set; }
        public string? HtmlContent { get; set; }
        public string? Provider { get; set; }
    }
}