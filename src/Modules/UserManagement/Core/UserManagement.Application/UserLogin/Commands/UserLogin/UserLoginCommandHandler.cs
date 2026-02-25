#nullable disable
using UserManagement.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging; // This is where the ILogger interface is defined
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Domain.Events;
using Serilog;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IUserSession;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using UserManagement.Domain.Entities;
using System.Collections.Concurrent;
namespace UserManagement.Application.UserLogin.Commands.UserLogin
{
    public class UserLoginCommandHandler : IRequestHandler<UserLoginCommand, ApiResponseDTO<LoginResponse>>
    {
        
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IJwtTokenHelper  _jwtTokenHelper;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IIPAddressService _ipAddressService;
         private readonly JwtSettings _jwtSettings;
        private readonly ILogger<UserLoginCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly ITimeZoneService _timeZoneService;
        private static readonly ConcurrentDictionary<string, UserLockoutInfo> _userLockoutInfo = new();
        private readonly ILoginPolicyFactory _loginPolicyFactory;

        public UserLoginCommandHandler( IJwtTokenHelper jwtTokenHelper, IUserQueryRepository userQueryRepository,
        IMediator mediator, ILogger<UserLoginCommandHandler> logger, IUserSessionRepository userSessionRepository,
        IHttpContextAccessor httpContextAccessor, IIPAddressService ipAddressService, IOptions<JwtSettings> jwtSettings, ITimeZoneService timeZoneService,
         ILoginPolicyFactory loginPolicyFactory)
        {
            
            _userQueryRepository = userQueryRepository;
            _jwtTokenHelper = jwtTokenHelper;
            _userSessionRepository = userSessionRepository;
            _httpContextAccessor = httpContextAccessor;
            _ipAddressService = ipAddressService;
            _jwtSettings = jwtSettings.Value;
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeZoneService = timeZoneService;
            _loginPolicyFactory = loginPolicyFactory;
        }

       public async Task<ApiResponseDTO<LoginResponse>> Handle(UserLoginCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling user login request for Username: {Username}", request.Username);

                        
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);
            
            var user = await _userQueryRepository.GetByUsernameAsync(request.Username);
            
            if (user == null)
            {
                return new ApiResponseDTO<LoginResponse>
                {
                    IsSuccess = false,
                    Message = "User Deactivated or does not exist Check with Admin."
                };
            }
          

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {

                //     _logger.LogWarning("Invalid login attempt for Username: {Username}", request.Username);

                //   var (remainingAttempts, lockoutTime) = await loginAttemptSession(user.UserName, currentTime);

                //     if (lockoutTime > 0)
                //     {
                //         return new ApiResponseDTO<LoginResponse>
                //         {
                //             IsSuccess = false,
                //             Message = $"User is locked. Try again after {lockoutTime:G}."
                //         };
                //     }     
                var policy = await _loginPolicyFactory.GetPolicyAsync(user);
                var message = await policy.CanAttemptLogin(user.UserName, DateTime.UtcNow);
                return new ApiResponseDTO<LoginResponse>
                {
                    IsSuccess = false,
                    Message = message
                };
            }

            // await _userSessionRepository.DeactivateUserSessionsAsync(user.UserId);   
                  
        

            
            // Generate JWT token            
 			var token = _jwtTokenHelper.GenerateToken(user.UserName,user.UserId,user.Mobile,user.EmailId,user.IsFirstTimeUser.ToString(),user.EntityId ?? 0,user.UserGroup.GroupCode,0,0,0,"",user.FirstName,user.LastName, out var jti);            var httpContext = _httpContextAccessor.HttpContext;
            var browserInfo = httpContext?.Request.Headers["User-Agent"].ToString();
            string broswerDetails = browserInfo != null ? _ipAddressService.GetUserBrowserDetails(browserInfo) : string.Empty;

            DateTime expirationTime = currentTime.AddMinutes(_jwtSettings.ExpiryMinutes);
            await _userSessionRepository.AddSessionAsync(new UserSessions
            {
                UserId = user.UserId,
                JwtId = jti,
                ExpiresAt = expirationTime, // Token expiry
                IsActive = 1,
                CreatedAt = currentTime,
                LastActivity = currentTime,
                BrowserInfo = broswerDetails
                
            });           
             _logger.LogInformation("JWT token generated for Username: {Username}", user.UserName);
            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Login",
                actionCode: user.UserName,
                actionName: "User logged in",
                details: $"User '{user.UserName}' logged in successfully with roles: {token}",
                module:"UserLogin"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            
            //Log login event via Serilog
            Log.Information("User {UserName} logged in successfully at {Time}. Roles: {Roles}", user.UserName, currentTime);

            _userLockoutInfo.TryRemove(request.Username, out _);

            return new ApiResponseDTO<LoginResponse>
            {
                IsSuccess = true,
                Message = "Login Successful.",
                Data = new LoginResponse
                {
                    Token = token,
                    IsFirstTimeUser = user.IsFirstTimeUser,
                    PartyId = user.PartyId,
                    Message = "Login Successful."
                }
            };
        } 
      
            
    }
}
