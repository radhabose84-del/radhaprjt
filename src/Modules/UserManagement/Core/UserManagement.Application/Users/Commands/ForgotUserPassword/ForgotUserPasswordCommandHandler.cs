#nullable disable
using AutoMapper;
using Contracts.Common;
using Contracts.Events.Notifications;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.INotifications;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.Utilities;
using UserManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Users.Queries.GetUsers;

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
        private readonly ITimeZoneService _timeZoneService;
        private readonly IBackgroundServiceClient _backgroundServiceClient;

        public ForgotUserPasswordCommandHandler(
            IUserQueryRepository userQueryRepository,
            IMapper mapper,
            IChangePassword changePasswordService,
            ILogger<ForgotUserPasswordCommandHandler> logger,
            INotificationsQueryRepository notificationsQueryRepository,
            IMediator mediator,
            ITimeZoneService timeZoneService,
            IBackgroundServiceClient backgroundServiceClient)
        {
            _userQueryRepository = userQueryRepository;
            _mapper = mapper;
            _changePasswordService = changePasswordService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationsQueryRepository = notificationsQueryRepository;
            _mediator = mediator;
            _timeZoneService = timeZoneService;
            _backgroundServiceClient = backgroundServiceClient;
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

            try
            {
                await _backgroundServiceClient.ScheduleVerificationCodeCleanupAsync(request.UserName, expiryMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Background service unavailable. Verification code cleanup for '{UserName}' will not be scheduled. " +
                    "Code will expire naturally at {ExpiryTime}.",
                    request.UserName,
                    ForgotPasswordCache.CodeStorage[request.UserName].ExpiryTime);
            }
            string provider = user.EmailId.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase)
                ? "Gmail"
                : "Zimbra";

            // Send email with verification code via MediatR (routes to SendEmailCommandHandler → RealEmailService → SMTP)
            try
            {
                var emailCommand = new SendEmailCommand
                {
                    ToEmail = user.EmailId,
                    Subject = "Password Reset - Verification Code",
                    HtmlContent = $@"<p>Dear {user.FirstName},</p>
<p>We received a request to reset your password.</p>
<p><b>Verification Code: {verificationCode}</b></p>
<p>This code is valid for <b>{expiryMinutes} minutes</b>.</p>
<p>If you did not request a password reset, please ignore this email.</p>
<p>Thanks,<br/>Support Team</p>",
                    Provider = provider
                };
                var emailSent = await _mediator.Send(emailCommand, cancellationToken);
                if (emailSent)
                    _logger.LogInformation("Password reset email sent to {Email} for user '{UserName}'.", user.EmailId, request.UserName);
                else
                    _logger.LogWarning("Failed to send password reset email to {Email} for user '{UserName}'.", user.EmailId, request.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Email sending failed for user '{UserName}'.", request.UserName);
            }

            // Send SMS with verification code via MediatR (routes to SendSmsCommandHandler → RealSmsService → SMS API)
            try
            {
                var smsCommand = new SendSmsCommand
                {
                    to = user.Mobile,
                    message = $"Dear {request.UserName}, We received a request to reset your password. Use the verification code below to proceed: Code: {verificationCode}. This code is valid for {expiryMinutes} minutes."
                };
                var smsSent = await _mediator.Send(smsCommand, cancellationToken);
                if (smsSent)
                    _logger.LogInformation("Password reset SMS sent to {Mobile} for user '{UserName}'.", user.Mobile, request.UserName);
                else
                    _logger.LogWarning("Failed to send password reset SMS to {Mobile} for user '{UserName}'.", user.Mobile, request.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SMS sending failed for user '{UserName}'.", request.UserName);
            }

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
