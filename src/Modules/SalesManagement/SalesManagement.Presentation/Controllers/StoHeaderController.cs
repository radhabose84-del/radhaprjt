using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.StoHeader.Commands.CreateStoHeader;
using SalesManagement.Application.StoHeader.Commands.DeleteStoHeader;
using SalesManagement.Application.StoHeader.Commands.UpdateStoHeader;
using SalesManagement.Application.StoHeader.Queries.GetAllStoHeader;
using SalesManagement.Application.StoHeader.Queries.GetPendingStoHeader;
using SalesManagement.Application.StoHeader.Queries.GetPendingStoHeaderById;
using SalesManagement.Application.StoHeader.Queries.GetStoHeaderAutoComplete;
using SalesManagement.Application.StoHeader.Queries.GetStoHeaderById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/sales/[controller]")]
    public class StoHeaderController : ApiControllerBase
    {
        public StoHeaderController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllStoHeaderAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllStoHeaderQuery
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
        public async Task<IActionResult> GetStoHeaderByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetStoHeaderByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingStoHeaderAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetPendingStoHeaderQuery
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

        [HttpGet("pending/{id}")]
        public async Task<IActionResult> GetPendingStoHeaderByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetPendingStoHeaderByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetStoHeaderAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetStoHeaderAutoCompleteQuery(term ?? string.Empty));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateStoHeader([FromBody] CreateStoHeaderCommand command)
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
        public async Task<IActionResult> UpdateStoHeader([FromBody] UpdateStoHeaderCommand command)
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
        public async Task<IActionResult> DeleteStoHeader(int id)
        {
            var result = await Mediator.Send(new DeleteStoHeaderCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }
    }
}
