#nullable disable
using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.Interfaces.IUserSession;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace UserManagement.Application.SwitchProfile.Commands.SwitchProfileByUnit
{
    public class SwitchProfileByUnitCommandHandler : IRequestHandler<SwitchProfileByUnitCommand, SwitchProfileByUnitDTO>
    {
        private readonly IJwtTokenHelper  _jwtTokenHelper;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMediator _mediator;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly JwtSettings _jwtSettings;
        public SwitchProfileByUnitCommandHandler( IJwtTokenHelper jwtTokenHelper,IUserSessionRepository userSessionRepository,IHttpContextAccessor httpContextAccessor,ITimeZoneService timeZoneService,IMediator mediator,IUserQueryRepository userQueryRepository,IIPAddressService ipAddressService,IOptions<JwtSettings> jwtSettings)
        {
            _jwtTokenHelper = jwtTokenHelper;
            _userSessionRepository = userSessionRepository;
            _httpContextAccessor = httpContextAccessor;
            _timeZoneService = timeZoneService;
            _mediator = mediator;
            _userQueryRepository = userQueryRepository;
            _ipAddressService = ipAddressService;
            _jwtSettings = jwtSettings.Value;
        }
        public async Task<SwitchProfileByUnitDTO> Handle(SwitchProfileByUnitCommand request, CancellationToken cancellationToken)
        {
            var userId = _ipAddressService.GetUserId();
            var groupCode = _ipAddressService.GetGroupCode();
            
            var user = await _userQueryRepository.GetByUserByUnit(userId,request.UnitId);
            if (user == null)
            {
                throw new ValidationException("User does not exist.");
               
            }
            var token = _jwtTokenHelper.GenerateToken(user.UserName,userId,user.Mobile,user.EmailId,user.IsFirstTimeUser.ToString(),user.EntityId ?? 0,groupCode,request.CompanyId,request.DivisionId,request.UnitId ,request.OldUnitId,user.FirstName,user.LastName, out var jti);   
           
            var httpContext = _httpContextAccessor.HttpContext;
            var browserInfo = httpContext?.Request.Headers["User-Agent"].ToString();
            string broswerDetails = browserInfo != null ? _ipAddressService.GetUserBrowserDetails(browserInfo) : string.Empty;
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);  
           
            DateTime expirationTime = currentTime.AddMinutes(_jwtSettings.ExpiryMinutes);
            await _userSessionRepository.DeactivateUserSessionsAsync(userId);
            await _userSessionRepository.AddSessionAsync(new UserSessions
            {
                UserId = user.UserId,
                JwtId = jti,
                ExpiresAt =expirationTime, 
                IsActive = 1,
                CreatedAt = currentTime,
                LastActivity =currentTime,
                BrowserInfo=broswerDetails
            }); 
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Profile",
                actionCode: "Profile",
                actionName: "User logged in",
                details: $"User Profile",
                module:"User"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return  new SwitchProfileByUnitDTO
                {
                    Token = token
                };
        }
    }
}