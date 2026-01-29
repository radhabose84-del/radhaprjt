using Core.Application.UserLogin.Commands.UserLogin;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using FluentValidation;
using Core.Application.Common.Interfaces.IUserSession;
using Microsoft.AspNetCore.Authorization;
using UserManagement.Infrastructure.Services;
using System.Collections.Concurrent;
using Hangfire;
using Core.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettingsById;
using Core.Application.Common.Interfaces.IUser;
using Infrastructure;
using Core.Application.Common.Interfaces;
using Core.Application.UserLogin.Commands.DeactivateUserSession;
using Core.Application.UserLogin.Commands.UnlockUser;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class AuthController : ControllerBase
    {
        
        private readonly IMediator _mediator;        
        private readonly IValidator<UserLoginCommand> _userLoginCommandValidator;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserSessionRepository _userSessionRepository;        
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IValidator<DeactivateUserSessionCommand> _deactivateUserSessionCommandValidator;

        public AuthController(IMediator mediator,IValidator<UserLoginCommand> userLoginCommandValidator,  ILogger<AuthController> logger,IUserSessionRepository userSessionRepository, IUserQueryRepository userQueryRepository, ITimeZoneService timeZoneService, IValidator<DeactivateUserSessionCommand> deactivateUserSessionCommandValidator)
        {
            _mediator = mediator;
            _userLoginCommandValidator = userLoginCommandValidator;
            _logger = logger;
            _userSessionRepository = userSessionRepository;            
            _userQueryRepository = userQueryRepository; 
            _timeZoneService = timeZoneService; 
            _deactivateUserSessionCommandValidator = deactivateUserSessionCommandValidator;          
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginCommand request)
        {  
             var SessisonValidate = await _userLoginCommandValidator.ValidateAsync(request, options => 
            {
              options.IncludeRuleSets("UserSession");
            });
            if (!SessisonValidate.IsValid)
            {
                return BadRequest(new 
                { 
                    StatusCode=StatusCodes.Status400BadRequest, 
                    message = "Validation failed", 
                    errors = SessisonValidate.Errors.Select(e => e.ErrorMessage).ToArray(),
                    session = true 
                });
            }
              var validationResult = await _userLoginCommandValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new 
                { 
                    StatusCode=StatusCodes.Status400BadRequest, 
                    message = "Validation failed", 
                    errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray(),
                    session = false  
                });
            }
                      
   
            var response = await _mediator.Send(request);   
            if (response.IsSuccess)
            {                
              
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = response.Message,
                    Data = response.Data
                });
            }
            _logger.LogWarning("Authentication failed for user: {Username}. Reason: {Message}", 
                request.Username, response.Message);

            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = response.Message
            }); 
        }
        
           
       // Get session by JWT ID
        [HttpGet("session/{jwtId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSessionByJwtId(string jwtId)
        {
            if (string.IsNullOrEmpty(jwtId))
            {
              //  return BadRequest(new { Message = "JWT ID cannot be null or empty." });
                 return Unauthorized(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "JWT ID cannot be null or empty."
                }); 
            }

            var session = await _userSessionRepository.GetSessionByJwtIdAsync(jwtId);

            if (session is null)
            {
                //return NotFound(new { Message = "Session not found for the provided JWT ID." });
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "JWT ID cannot be null or empty."
                }); 
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Session retrieved successfully.",
                Data = session
            });
        }

        // Deactivate expired sessions
        [HttpPost("deactivate-expired")]
        [AllowAnonymous]
        public async Task<IActionResult> DeactivateExpiredSessions()
        {
            await _userSessionRepository.DeactivateExpiredSessionsAsync();

            _logger.LogInformation("Expired sessions have been deactivated.");

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Expired sessions have been deactivated."
            });
        }
      
                        // Deactivate user sessions by User ID
        [HttpPost("deactivate-user-session/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeactivateUserSessionsAsync(int userId)
        {
            await _userSessionRepository.DeactivateUserSessionsAsync(userId);

            _logger.LogInformation("All sessions for user {UserId} have been deactivated.", userId);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = $"All sessions for user {userId} have been deactivated."
            });
        }
         [HttpPost("deactivate-user-sessionByUsername")]
        [AllowAnonymous]
        public async Task<IActionResult> DeactivateUserSessionsByUsername([FromBody] DeactivateUserSessionCommand command)
        {
              var validationResult = await _deactivateUserSessionCommandValidator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return BadRequest(new 
                { 
                    StatusCode=StatusCodes.Status400BadRequest, 
                    message = "Validation failed", 
                    errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray() 
                });
            }
            var UserSession = await _mediator.Send(command);
            

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = $"All sessions for user {command.Username} have been deactivated."
            });
        }
         [HttpPost("unlock")]
        [AllowAnonymous]
        public async Task<IActionResult> UnlockUser([FromBody] UnlockUserCommand command)
        {
            var UserSession = await _mediator.Send(command);          

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = $"All sessions for user {command.userName} have been deactivated."
            });
        }
    }
}

