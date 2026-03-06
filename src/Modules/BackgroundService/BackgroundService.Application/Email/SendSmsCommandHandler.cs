using BackgroundService.Application.Notification.Common.Interfaces;
using Contracts.Events.Notifications;
using MediatR;


namespace BackgroundService.Application.Email
{
    public class SendSmsCommandHandler   : IRequestHandler<SendSmsCommand, bool>
    {
        private readonly ISmsService _smsService;

        public SendSmsCommandHandler(ISmsService smsService)
        {
            _smsService = smsService;
        }

        public async Task<bool> Handle(SendSmsCommand request, CancellationToken cancellationToken)
        {
            //return await _emailService.SendEmailAsync(request.ToEmail!, request.Subject!, request.HtmlContent!, request.Provider ?? "Gmail");
            return await _smsService.SendSmsAsync(request);
        } 
        
   
    }
}