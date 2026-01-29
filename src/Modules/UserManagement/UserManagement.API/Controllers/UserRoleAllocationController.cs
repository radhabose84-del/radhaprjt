using Core.Application.DeleteUserRoleAllocation.Commands.DeleteUserRoleAllocation;
using Core.Application.UserRoleAllocation.Commands.CreateUserRoleAllocation;
using Core.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;
using Core.Application.UserRoleAllocation.Queries.GetUserRoleAllocationById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class UserRoleAllocationController : ApiControllerBase
    {
        public UserRoleAllocationController(ISender mediator) : base(mediator)
        {
        }

    [HttpPost]
    public async Task<IActionResult> CreateRoleAllocation([FromBody] CreateUserRoleAllocationDto dto)
    {
        var command = new CreateUserRoleAllocationCommand(dto);
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoleAllocation(int id)
    {
        var command = new DeleteRoleAllocationCommand(id);
        var result = await Mediator.Send(command);

        if (result == 0)
        {
            return NotFound($"Role allocation with ID {id} not found.");
        }

        return NoContent(); 
    }

    [HttpGet]
    public async Task<IActionResult> GetUserRoleAllocations()
    {
        var query = new GetUserRoleAllocationQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }

        [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserRoleAllocationById(int userId)
    {
        var query = new GetUserRoleAllocationByIdQuery(userId);
        var result = await Mediator.Send(query);

        if (result == null)
        {
            return NotFound($"No role allocations found for user with ID {userId}.");
        }

        return Ok(result);
    }

    }
}