using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.INotifications;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.Utilities;
using UserManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Users.Queries.GetUsers;
// using Contracts.Events.Notifications;

namespace UserManagement.Application.Users.Commands.ForgotUserPassword
{
    public class ForgotUserPasswordCommandHandler : IRequestHandler<ForgotUserPasswordCommand, ApiResponseDTO<ForgotPasswordResponse>>
    {
        private readonly IMapper _mapper;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IChangePassword _changePasswordService;
        private readonly IMediator _mediator;
        private readonly INotificationsQueryRepository _notificationsQueryRepository;
        private readonly ILogger<ForgotUserPasswordCommandHandler> _logger;        
        // private readonly ISmsService _smsService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IBackgroundServiceClient  _backgroundServiceClient;

        public ForgotUserPasswordCommandHandler(
            IUserQueryRepository userQueryRepository,
            IMapper mapper,
            IChangePassword changePasswordService,
            ILogger<ForgotUserPasswordCommandHandler> logger,
            INotificationsQueryRepository notificationsQueryRepository,
            IMediator mediator
            // ,ISmsService smsService
            , ITimeZoneService timeZoneService, IBackgroundServiceClient backgroundServiceClient)
        {
            _userQueryRepository = userQueryRepository;
            _mapper = mapper;
            _changePasswordService = changePasswordService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationsQueryRepository = notificationsQueryRepository;
            _mediator = mediator;            
            // _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _timeZoneService = timeZoneService;    
            _backgroundServiceClient=backgroundServiceClient;
        }

        public async Task<ApiResponseDTO<ForgotPasswordResponse>> Handle(ForgotUserPasswordCommand request, CancellationToken cancellationToken)
        {
            // Fetch user details
            var user = await _userQueryRepository.GetByUsernameAsync(request.UserName);

            // Generate verification code
            string verificationCode = await _changePasswordService.GenerateVerificationCode(6);
            int expiryMinutes = await _notificationsQueryRepository.GetResetCodeExpiryMinutes();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId); 
             ForgotPasswordCache.CodeStorage[request.UserName] = new VerificationCodeDetails
            {
                Code = verificationCode,
                ExpiryTime = currentTime.AddMinutes(expiryMinutes)
            };

            await _backgroundServiceClient.ScheduleVerificationCodeCleanupAsync(request.UserName, expiryMinutes);
            string provider = user.EmailId.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase)
                ? "Gmail"
                : "Zimbra";

            //SMS
            // var smsCommand = new SendSmsCommand
            // {
            //     to = user.Mobile,
            //     message = $"Dear {request.UserName}, We received a request to reset your password. Use the verification code below to proceed:Code:{verificationCode}, This code is valid for {expiryMinutes} minutes."
                
            // };
            // var smsSent=await _smsService.SendSmsAsync(smsCommand);
          

            // if (smsSent)
            // {
            //     _logger.LogInformation("Login notification SMS sent to {Mobile}.",  user.Mobile);
            // }
            // else
            // {
            //     _logger.LogWarning("Failed to send login notification SMS to {Mobile}.", user.Mobile);
            // }  
            
            // Publish domain event for logging purposes
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "ResetUserPassword",
                actionCode: verificationCode,
                actionName: request.UserName,
                details: $"Username '{request.UserName}' requested a password reset. Verification Code: {verificationCode}",
                module: "ResetUserPassword"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            // Create response
            var userDto = _mapper.Map<UserDto>(user);
            var responseDto = new ForgotPasswordResponse
            {
                Message = $"Verification code sent to your registered email address {userDto.EmailId} and mobile number {userDto.Mobile}.",
                Email = userDto.EmailId,
                Mobile = userDto.Mobile,
                VerificationCode = verificationCode,
                PasswordResetCodeExpiryMinutes = expiryMinutes
            };

            _logger.LogInformation($"Verification code sent successfully for username '{userDto.UserName}'.");
            return new ApiResponseDTO<ForgotPasswordResponse> { IsSuccess = true, Data = responseDto };
        }      
    }
}
