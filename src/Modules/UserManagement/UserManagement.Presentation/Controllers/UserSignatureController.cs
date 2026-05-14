using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.UserSignature.Command.CreateUserSignature;
using UserManagement.Application.UserSignature.Command.DeleteUserSignature;
using UserManagement.Application.UserSignature.Command.UpdateUserSignature;
using UserManagement.Application.UserSignature.Queries.GetAllUserSignature;
using UserManagement.Application.UserSignature.Queries.GetUserSignatureById;
using UserManagement.Application.UserSignature.Queries.GetUserSignatureByUserId;
using UserManagement.Presentation.Requests.UserSignature;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class UserSignatureController : ApiControllerBase
    {
        public UserSignatureController(ISender mediator) : base(mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUserSignatureAsync(
            [FromQuery] int PageNumber = 1,
            [FromQuery] int PageSize = 15,
            [FromQuery] string? SearchTerm = null)
        {
            var response = await Mediator.Send(new GetAllUserSignatureQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            if (response.Data == null || response.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = response.Message
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = response.Data,
                TotalCount = response.TotalCount,
                PageNumber = response.PageNumber,
                PageSize = response.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetUserSignatureByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = result
            });
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUserIdAsync(int userId)
        {
            var result = await Mediator.Send(new GetUserSignatureByUserIdQuery { UserId = userId });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = result
            });
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateAsync([FromForm] CreateUserSignatureRequest request)
        {
            var command = new CreateUserSignatureCommand
            {
                UserId = request.UserId,
                File = request.File
            };

            var newId = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "UserSignature created successfully.",
                Data = newId
            });
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] UpdateUserSignatureRequest request)
        {
            var command = new UpdateUserSignatureCommand
            {
                Id = id,
                File = request.File,
                IsActive = request.IsActive == 1 ? Status.Active : Status.Inactive
            };

            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "UserSignature updated successfully.",
                Data = result
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await Mediator.Send(new DeleteUserSignatureCommand { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "UserSignature deleted successfully."
            });
        }
    }
}
