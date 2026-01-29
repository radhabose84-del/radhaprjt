using MediatR;

namespace Contracts.Events.Notifications
{
    public class SendSmsCommand : IRequest<bool>
    {
        public string? to { get; set; }    
        public string? message { get; set; }        
    }
}