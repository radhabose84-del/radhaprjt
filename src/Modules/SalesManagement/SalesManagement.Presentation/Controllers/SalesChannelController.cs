#nullable disable
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesChannel.Commands.CreateSalesChannel;
using SalesManagement.Application.SalesChannel.Commands.UpdateSalesChannel;
using SalesManagement.Application.SalesChannel.Commands.DeleteSalesChannel;
using SalesManagement.Application.SalesChannel.Queries.GetAllSalesChannel;
using SalesManagement.Application.SalesChannel.Queries.GetSalesChannelById;
using SalesManagement.Application.SalesChannel.Queries.GetSalesChannelAutoComplete;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class SalesChannelController : ApiControllerBase
    {
        public SalesChannelController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllSalesChannelAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllSalesChannelQuery
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
        public async Task<IActionResult> GetSalesChannelByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetSalesChannelByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetSalesChannelAutoCompleteAsync([FromQuery] string term = null)
        {
            var result = await Mediator.Send(new GetSalesChannelAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalesChannel([FromBody] CreateSalesChannelCommand command)
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
        public async Task<IActionResult> UpdateSalesChannel([FromBody] UpdateSalesChannelCommand command)
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
        public async Task<IActionResult> DeleteSalesChannel(int id)
        {
            var result = await Mediator.Send(new DeleteSalesChannelCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Sales Channel deleted successfully." : "Failed to delete Sales Channel."
            });
        }
    }
}
