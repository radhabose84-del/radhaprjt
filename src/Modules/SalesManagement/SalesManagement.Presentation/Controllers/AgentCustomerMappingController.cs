using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.AgentCustomerMapping.Commands.CreateAgentCustomerMapping;
using SalesManagement.Application.AgentCustomerMapping.Commands.DeleteAgentCustomerMapping;
using SalesManagement.Application.AgentCustomerMapping.Commands.UpdateAgentCustomerMapping;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingAutoComplete;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingByCustomerId;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingById;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingByFilter;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetAllAgentCustomerMapping;
using SalesManagement.Application.AgentCustomerMapping.Queries.GetCustomersByMarketingOfficerId;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class AgentCustomerMappingController : ApiControllerBase
    {
        public AgentCustomerMappingController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAgentCustomerMappingAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllAgentCustomerMappingQuery
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
        public async Task<IActionResult> GetAgentCustomerMappingByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetAgentCustomerMappingByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAgentCustomerMappingAutoCompleteAsync(
            [FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetAgentCustomerMappingAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-customer/{customerId}")]
        public async Task<IActionResult> GetAgentCustomerMappingByCustomerIdAsync(int customerId)
        {
            var result = await Mediator.Send(new GetAgentCustomerMappingByCustomerIdQuery { CustomerId = customerId });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }

        [HttpGet("by-filter")]
        public async Task<IActionResult> GetAgentCustomerMappingByFilterAsync(
            [FromQuery] int? salesGroupId = null,
            [FromQuery] int? customerId = null)
        {
            var result = await Mediator.Send(new GetAgentCustomerMappingByFilterQuery
            {
                SalesGroupId = salesGroupId,
                CustomerId = customerId
            });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }

        [HttpGet("by-marketing-officer/{marketingOfficerId}")]
        public async Task<IActionResult> GetCustomersByMarketingOfficerIdAsync(int marketingOfficerId)
        {
            var result = await Mediator.Send(new GetCustomersByMarketingOfficerIdQuery { MarketingOfficerId = marketingOfficerId });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAgentCustomerMapping(
            [FromBody] CreateAgentCustomerMappingCommand command)
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
        public async Task<IActionResult> UpdateAgentCustomerMapping(
            [FromBody] UpdateAgentCustomerMappingCommand command)
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
        public async Task<IActionResult> DeleteAgentCustomerMapping(int id)
        {
            var result = await Mediator.Send(new DeleteAgentCustomerMappingCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Agent Customer Mapping deleted successfully." : "Agent Customer Mapping not found."
            });
        }
    }
}
