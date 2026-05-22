using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.AccessPolicy.Commands.AssignRoleAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.CreateAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.DeleteAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.RemoveRoleAccessPolicy;
using UserManagement.Application.AccessPolicy.Commands.UpdateAccessPolicy;
using UserManagement.Application.AccessPolicy.Queries.GetAccessPolicyAutoComplete;
using UserManagement.Application.AccessPolicy.Queries.GetAccessPolicyById;
using UserManagement.Application.AccessPolicy.Queries.GetAllAccessPolicy;
using UserManagement.Application.AccessPolicy.Queries.GetRoleAccessPolicies;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/usermanagement/[controller]")]
    public class AccessPolicyController : ApiControllerBase
    {
        public AccessPolicyController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAccessPolicyAsync(
            [FromQuery] int     PageNumber,
            [FromQuery] int     PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllAccessPolicyQuery
            {
                PageNumber = PageNumber,
                PageSize   = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data       = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize   = result.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccessPolicyByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetAccessPolicyByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data       = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAccessPolicyAutoCompleteAsync(
            [FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetAccessPolicyAutoCompleteQuery(term));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data       = result
            });
        }

        [HttpGet("{id}/role-assignments")]
        public async Task<IActionResult> GetRoleAccessPoliciesAsync(int id, [FromQuery] int? roleId = null)
        {
            var result = await Mediator.Send(new GetRoleAccessPoliciesQuery
            {
                AccessPolicyId = id,
                RoleId         = roleId
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data       = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccessPolicy([FromBody] CreateAccessPolicyCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess  = result.IsSuccess,
                message    = result.Message,
                data       = result.Data
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAccessPolicy([FromBody] UpdateAccessPolicyCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess  = result.IsSuccess,
                message    = result.Message,
                data       = result.Data
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccessPolicy([FromQuery] int id)
        {
            var result = await Mediator.Send(new DeleteAccessPolicyCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess  = result,
                message    = result ? "Access Policy deleted successfully." : "Access Policy not found."
            });
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRoleAccessPolicy([FromBody] AssignRoleAccessPolicyCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess  = result.IsSuccess,
                message    = result.Message,
                data       = result.Data
            });
        }

        [HttpDelete("remove-role/{id}")]
        public async Task<IActionResult> RemoveRoleAccessPolicy(int id)
        {
            var result = await Mediator.Send(new RemoveRoleAccessPolicyCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess  = result,
                message    = result ? "Role access policy removed successfully." : "Role access policy not found."
            });
        }
    }
}
