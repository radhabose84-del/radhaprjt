#nullable disable
using UserManagement.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Users.Queries.GetUsers;
using UserManagement.Application.Users.Queries.GetUserById;
using UserManagement.Application.Users.Commands.CreateUser;
using UserManagement.Application.Users.Commands.UpdateUser;
using UserManagement.Application.Users.Commands.DeleteUser;
using UserManagement.Application.Users.Queries.GetUserAutoComplete;
using UserManagement.Application.Users.Commands.UpdateFirstTimeUserPassword;
using UserManagement.Application.Users.Commands.ChangeUserPassword;
using Microsoft.AspNetCore.Authorization;
using UserManagement.Application.Users.Commands.ForgotUserPassword;
using UserManagement.Application.Users.Commands.ResetUserPassword;
using MassTransit;
using UserManagement.Application.Users.Commands.RemoveVerificationCode;
using Microsoft.Extensions.Logging;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]

    public class UserController : ApiControllerBase
    {
        private readonly IValidator<CreateUserCommand> _createUserCommandValidator;
        private readonly IValidator<UpdateUserCommand> _updateUserCommandValidator;
        private readonly IValidator<FirstTimeUserPasswordCommand> _firstTimeUserPasswordCommandValidator;
        private readonly IValidator<ChangeUserPasswordCommand> _changeUserPasswordCommandValidator;
        private readonly ILogger<UserController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IValidator<ForgotUserPasswordCommand> _forgotUserPasswordCommandValidator;
        private readonly IValidator<ResetUserPasswordCommand> _resetUserPasswordCommandValidator;

        public UserController(ISender mediator,
                              IValidator<CreateUserCommand> createUserCommandValidator,
                              IValidator<UpdateUserCommand> updateUserCommandValidator,
                              ApplicationDbContext dbContext,
                              IValidator<FirstTimeUserPasswordCommand> firstTimeUserPasswordCommandValidator,
                              IValidator<ChangeUserPasswordCommand> changeUserPasswordCommandValidator,
                              ILogger<UserController> logger,
                              IValidator<ForgotUserPasswordCommand> forgotUserPasswordCommandValidator,
                              IValidator<ResetUserPasswordCommand> resetUserPasswordCommandValidator,
                              IPublishEndpoint publishEndpoint)
          : base(mediator)
        {
            _createUserCommandValidator = createUserCommandValidator;
            _updateUserCommandValidator = updateUserCommandValidator;
            _firstTimeUserPasswordCommandValidator = firstTimeUserPasswordCommandValidator;
            _changeUserPasswordCommandValidator = changeUserPasswordCommandValidator;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _forgotUserPasswordCommandValidator = forgotUserPasswordCommandValidator;
            _resetUserPasswordCommandValidator = resetUserPasswordCommandValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsersAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string SearchTerm = null)
        {
            var users = await Mediator.Send(new GetUserQuery
            {

                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });
            // var activeUsers = users.ToList();

            if (!users.IsSuccess)
            {

                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = users.Message
                });
            }

            _logger.LogInformation($"Total {users.Data.Count} active users listed successfully.");


            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = users.Data,
                TotalCount = users.TotalCount,
                PageNumber = users.PageNumber,
                PageSize = users.PageSize

            });
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {

            var user = await Mediator.Send(new GetUserByIdQuery { UserId = id });


            if (user is null)
            {

                _logger.LogWarning($"User not found for ID {id}.");


                return NotFound(new { StatusCode = StatusCodes.Status404NotFound, message = $"User ID {id} not found." });
            }

            _logger.LogWarning("User Listed successfully: {Username}", user);


            return Ok(new { StatusCode = StatusCodes.Status200OK, data = user });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateUserCommand createUserCommand)
        {

            var validationResult = await _createUserCommandValidator.ValidateAsync(createUserCommand);

            _logger.LogWarning($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Validation failed",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            var response = await Mediator.Send(createUserCommand);
            if (response.IsSuccess)
            {


                _logger.LogInformation($"User {createUserCommand.UserName} created successfully.");
                return Ok(new { StatusCode = StatusCodes.Status201Created, message = response.Message, data = response.Data });
            }


            _logger.LogWarning($"Failed to create user {createUserCommand.UserName}.");



            return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, message = response.Message });
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateUserCommand updateUserCommand)
        {


            var validationResult = await _updateUserCommandValidator.ValidateAsync(updateUserCommand);

            _logger.LogWarning($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Validation failed",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            var userExists = await Mediator.Send(new GetUserByIdQuery { UserId = updateUserCommand.UserId });
            if (userExists is null)
            {


                _logger.LogInformation($"User with ID {updateUserCommand.UserId} not found for update.");
                return NotFound(new { StatusCode = StatusCodes.Status404NotFound, message = $"User ID {updateUserCommand.UserId} not found." });
            }


            var response = await Mediator.Send(updateUserCommand);
            if (response.IsSuccess)
            {

                _logger.LogInformation($"User {updateUserCommand.UserName} updated successfully.");
                return Ok(new { StatusCode = StatusCodes.Status200OK, message = response.Message });
            }


            _logger.LogWarning($"Failed to update user {updateUserCommand.UserName}.");


            return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, message = response.Message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {

            if (id <= 0)
            {
                return BadRequest(new

                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid User ID"
                });
            }

            var result = await Mediator.Send(new DeleteUserCommand { UserId = id });
            if (!result.IsSuccess)

            {
                _logger.LogWarning($"Deletion failed for User {id}: {result?.Message ?? "Unknown error"}.");

                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = result.Message
                });
            }
            _logger.LogInformation($"User {id} deleted successfully.");



            return Ok(new
            {

                StatusCode = StatusCodes.Status200OK,

                data = $"User ID {id} Deleted"
            });
            // var deleteUser = await Mediator.Send(deleteUserCommand);


            // if(deleteUser.IsSuccess)
            // {
            //     _logger.LogInformation($"User {deleteUserCommand.UserId} deleted successfully.");
            //     return Ok(new { StatusCode=StatusCodes.Status200OK, message = deleteUser.Message, errors = "" });

            // }
            //     _logger.LogInformation($"Failed to delete user with ID {deleteUserCommand.UserId}.");
            //     return BadRequest(new { StatusCode=StatusCodes.Status400BadRequest, message = deleteUser.Message, errors = "" });
        }

        [HttpGet]
        [Route("by-name")]
        public async Task<IActionResult> GetByUsernameAsync([FromQuery] string name)
        {
            var users = await Mediator.Send(new GetUserAutoCompleteQuery { SearchPattern = name });
            _logger.LogWarning($"Users listed successfully: {string.Join(", ", users)}");

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = users.Data });
        }

        [HttpPut("password/first-time")]
        public async Task<IActionResult> FirstTimeUserChangePassword([FromBody] FirstTimeUserPasswordCommand firstTimeUserPasswordCommand)
        {

            var validationResult = await _firstTimeUserPasswordCommandValidator.ValidateAsync(firstTimeUserPasswordCommand);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Validation failed",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }



            var response = await Mediator.Send(firstTimeUserPasswordCommand);
            if (!response.IsSuccess)
            {
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, message = response.Message });
            }
            _logger.LogInformation($"First Time User {firstTimeUserPasswordCommand.UserName} and Password changed successfully.");

            return Ok(new { StatusCode = StatusCodes.Status200OK, message = response });
        }

        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangeUserPasswordCommand changeUserPasswordCommand)
        {

            var validationResult = await _changeUserPasswordCommandValidator.ValidateAsync(changeUserPasswordCommand);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Validation failed",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            var response = await Mediator.Send(changeUserPasswordCommand);
            if (response.IsSuccess)
            {

                _logger.LogInformation($"User {changeUserPasswordCommand.UserName} and password changed successfully.");

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    message = response
                });
            }
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = response.Message
            });

        }

        [HttpPost("password/reset-request")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotUserPassword([FromBody] ForgotUserPasswordCommand forgotUserPasswordCommand)

        {
            var validationResult = await _forgotUserPasswordCommandValidator.ValidateAsync(forgotUserPasswordCommand);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Validation failed",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            var response = await Mediator.Send(forgotUserPasswordCommand);

            if (response.IsSuccess)
            {

                _logger.LogInformation($"User {forgotUserPasswordCommand.UserName} fetched successfully.");



                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    message = response.Data, // Correctly access the message

                });
            }
            _logger.LogWarning($"Invalid username, email, or mobile number: {forgotUserPasswordCommand.UserName}.");


            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = response.Message // Access the message for error
            });
        }

        [HttpPut("password/reset")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetUserPassword([FromBody] ResetUserPasswordCommand resetUserPasswordCommand)
        {

            var validationResult = await _resetUserPasswordCommandValidator.ValidateAsync(resetUserPasswordCommand);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Validation failed",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            var response = await Mediator.Send(resetUserPasswordCommand);
            if (!response.IsSuccess)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = response.Message
                });
            }
            _logger.LogInformation($"Password changed successfully for user {resetUserPasswordCommand.UserName}.");


            return Ok(new { StatusCode = StatusCodes.Status200OK, message = response });
        }
        [HttpPost("rollback-delete")]
        public async Task<IActionResult> RollbackDeleteUser([FromBody] DeleteUserCommand command)
        {
            var result = await Mediator.Send(command);
            return result.IsSuccess ? Ok(result) : StatusCode(500, result);
        }
        [HttpPost("verfication-code-remove")]
        [AllowAnonymous]
        public async Task<IActionResult> VerificationCodeRemove([FromBody] RemoveVerficationCodeCommand command)
        {
            var result = await Mediator.Send(command);
            return result.IsSuccess ? Ok(result) : StatusCode(500, result);
        }
    }
}