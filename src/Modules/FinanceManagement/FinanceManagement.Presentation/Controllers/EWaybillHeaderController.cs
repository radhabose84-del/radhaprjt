using FinanceManagement.Application.EWaybillHeader.Commands.CreateEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Commands.DeleteEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Commands.UpdateEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Queries.GetAllEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Queries.GetEWaybillHeaderAutoComplete;
using FinanceManagement.Application.EWaybillHeader.Queries.GetEWaybillHeaderById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class EWaybillHeaderController : ApiControllerBase
    {
        public EWaybillHeaderController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllEWaybillHeaderAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllEWaybillHeaderQuery
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
        public async Task<IActionResult> GetEWaybillHeaderByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetEWaybillHeaderByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetEWaybillHeaderAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetEWaybillHeaderAutoCompleteQuery(term ?? string.Empty));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateEWaybillHeader([FromBody] CreateEWaybillHeaderCommand command)
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
        public async Task<IActionResult> UpdateEWaybillHeader([FromBody] UpdateEWaybillHeaderCommand command)
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
        public async Task<IActionResult> DeleteEWaybillHeader(int id)
        {
            var result = await Mediator.Send(new DeleteEWaybillHeaderCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "EWaybill Header deleted successfully." : "EWaybill Header not found."
            });
        }
    }
}
