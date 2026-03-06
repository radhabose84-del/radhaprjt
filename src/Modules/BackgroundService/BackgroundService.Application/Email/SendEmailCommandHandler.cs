using BackgroundService.Application.Notification.Common.Interfaces;
using Contracts.Events.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Email
{
    public class SendEmailCommandHandler  : IRequestHandler<SendEmailCommand, bool>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<SendEmailCommandHandler> _logger;

        public SendEmailCommandHandler(IEmailService emailService, ILogger<SendEmailCommandHandler> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }
        public async Task<bool> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
             _logger.LogInformation("✅ from SendEmailCommandHandler");           
            //return await _emailService.SendEmailAsync(request.ToEmail!, request.Subject!, request.HtmlContent!, request.Provider ?? "Gmail");
            return await _emailService.SendEmailAsync(request);
        }   
    }
}