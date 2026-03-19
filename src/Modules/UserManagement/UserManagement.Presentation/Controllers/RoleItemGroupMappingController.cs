#nullable disable
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.RoleItemGroupMapping.Commands.CreateRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.DeleteRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.UpdateRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Queries.GetAllRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Queries.GetRoleItemGroupMappingById;
using UserManagement.Application.RoleItemGroupMapping.Queries.GetRoleItemGroupMappingByRoleId;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class RoleItemGroupMappingController : ApiControllerBase
    {
        public RoleItemGroupMappingController(ISender mediator)
            : base(mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllRoleItemGroupMappingQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result.Message,
                data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid RoleItemGroupMapping ID"
                });
            }

            var result = await Mediator.Send(new GetRoleItemGroupMappingByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-role/{roleId}")]
        public async Task<IActionResult> GetByRoleIdAsync(int roleId)
        {
            if (roleId <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid Role ID"
                });
            }

            var result = await Mediator.Send(new GetRoleItemGroupMappingByRoleIdQuery { RoleId = roleId });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateRoleItemGroupMappingCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "RoleItemGroupMapping created successfully.",
                data = result
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateRoleItemGroupMappingCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "RoleItemGroupMapping updated successfully.",
                data = result
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid RoleItemGroupMapping ID"
                });
            }

            var result = await Mediator.Send(new DeleteRoleItemGroupMappingCommand { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = $"RoleItemGroupMapping ID {id} deleted.",
                message = result
            });
        }
    }
}
