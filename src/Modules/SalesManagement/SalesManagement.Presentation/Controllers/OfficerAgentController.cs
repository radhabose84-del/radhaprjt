using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.OfficerAgent.Commands.CreateOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.UpdateOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.DeleteOfficerAgent;
using SalesManagement.Application.OfficerAgent.Queries.GetAllOfficerAgent;
using SalesManagement.Application.OfficerAgent.Queries.GetOfficerAgentById;
using SalesManagement.Application.OfficerAgent.Queries.GetOfficerAgentAutoComplete;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class OfficerAgentController : ApiControllerBase
    {
        public OfficerAgentController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllOfficerAgentAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllOfficerAgentQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOfficerAgentByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetOfficerAgentByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetOfficerAgentAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetOfficerAgentAutoCompleteQuery(term));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateOfficerAgent([FromBody] CreateOfficerAgentCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOfficerAgent([FromBody] UpdateOfficerAgentCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteOfficerAgent(int id)
        {
            var result = await Mediator.Send(new DeleteOfficerAgentCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Officer Agent assignment deleted successfully." : "Failed to delete Officer Agent assignment."
            });
        }
    }
}
