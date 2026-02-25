#nullable disable
using AutoMapper;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.INotifications;
using UserManagement.Application.Common.Utilities;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.EntityLevelAdmin.Commands.SendOTP
{
    public class SendOTPCommandHandler : IRequestHandler<SendOTPCommand, SendOTPDTO>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IChangePassword _changePasswordService;
        private readonly INotificationsQueryRepository _notificationsQueryRepository;
        private readonly ITimeZoneService _timeZoneService;
        public SendOTPCommandHandler(IMediator mediator, IMapper mapper, IChangePassword changePasswordService, INotificationsQueryRepository notificationsQueryRepository, ITimeZoneService timeZoneService)
        {
            _mediator = mediator;
            _mapper = mapper;
            _changePasswordService = changePasswordService;
            _notificationsQueryRepository = notificationsQueryRepository;
            _timeZoneService = timeZoneService;
        }

        public async Task<SendOTPDTO> Handle(SendOTPCommand request, CancellationToken cancellationToken)
        {
            string verificationCode = await _changePasswordService.GenerateVerificationCode(6);
            int expiryMinutes = await _notificationsQueryRepository.GetResetCodeExpiryMinutes();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId); 

             ForgotPasswordCache.CodeStorage[request.Email] = new VerificationCodeDetails
            {
                Code = verificationCode,
                ExpiryTime = currentTime.AddMinutes(expiryMinutes)
            };

             Hangfire.BackgroundJob.Schedule(
            () => ForgotPasswordCache.RemoveVerificationCode(request.Email), 
            TimeSpan.FromMinutes(expiryMinutes)
            );

             var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "SendOTP to Entity Admin",
                actionCode: "OTP",
                actionName: "Genrate OTP",
                details: $"Username '{request.Email}' requested a password reset. Verification Code: {verificationCode}",
                module: "User"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return  new SendOTPDTO 
                { 
                    Email = request.Email,
                    PasswordResetCodeExpiryMinutes = expiryMinutes,
                    VerificationCode = verificationCode
                };
        }
    }
}