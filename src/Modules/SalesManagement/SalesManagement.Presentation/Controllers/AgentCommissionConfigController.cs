using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.AgentCommissionConfig.Commands.CreateAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Commands.UpdateAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Commands.DeleteAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Queries.GetAllAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Queries.GetAgentCommissionConfigById;
using SalesManagement.Application.AgentCommissionConfig.Queries.GetAgentCommissionConfigAutoComplete;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class AgentCommissionConfigController : ApiControllerBase
    {
        public AgentCommissionConfigController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAgentCommissionConfigAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllAgentCommissionConfigQuery
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
        public async Task<IActionResult> GetAgentCommissionConfigByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetAgentCommissionConfigByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAgentCommissionConfigAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetAgentCommissionConfigAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAgentCommissionConfig([FromBody] CreateAgentCommissionConfigCommand command)
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
        public async Task<IActionResult> UpdateAgentCommissionConfig([FromBody] UpdateAgentCommissionConfigCommand command)
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
        public async Task<IActionResult> DeleteAgentCommissionConfig(int id)
        {
            var result = await Mediator.Send(new DeleteAgentCommissionConfigCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Agent Commission Configuration deleted successfully." : "Failed to delete Agent Commission Configuration."
            });
        }
    }
}
