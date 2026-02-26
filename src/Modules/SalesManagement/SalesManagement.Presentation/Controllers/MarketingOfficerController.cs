using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.MarketingOfficer.Commands.CreateMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Commands.DeleteMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Commands.UpdateMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Queries.GetAllMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Queries.GetMarketingOfficerAutoComplete;
using SalesManagement.Application.MarketingOfficer.Queries.GetMarketingOfficerById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class MarketingOfficerController : ApiControllerBase
    {
        public MarketingOfficerController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllMarketingOfficerAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllMarketingOfficerQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                result.TotalCount,
                result.PageNumber,
                result.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMarketingOfficerByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetMarketingOfficerByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetMarketingOfficerAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetMarketingOfficerAutoCompleteQuery(term ?? string.Empty));

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateMarketingOfficer([FromBody] CreateMarketingOfficerCommand command)
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
        public async Task<IActionResult> UpdateMarketingOfficer([FromBody] UpdateMarketingOfficerCommand command)
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
        public async Task<IActionResult> DeleteMarketingOfficer(int id)
        {
            var result = await Mediator.Send(new DeleteMarketingOfficerCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Marketing Officer deleted successfully." : "Failed to delete Marketing Officer.",
                data = result
            });
        }
    }
}
