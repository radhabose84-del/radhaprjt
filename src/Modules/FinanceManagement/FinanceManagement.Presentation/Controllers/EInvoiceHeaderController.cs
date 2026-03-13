using FinanceManagement.Application.EInvoiceHeader.Commands.CreateEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.DeleteEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.UpdateEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetAllEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetEInvoiceHeaderAutoComplete;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetEInvoiceHeaderById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class EInvoiceHeaderController : ApiControllerBase
    {
        public EInvoiceHeaderController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllEInvoiceHeaderAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllEInvoiceHeaderQuery
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
        public async Task<IActionResult> GetEInvoiceHeaderByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetEInvoiceHeaderByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetEInvoiceHeaderAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetEInvoiceHeaderAutoCompleteQuery(term ?? string.Empty));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateEInvoiceHeader([FromBody] CreateEInvoiceHeaderCommand command)
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
        public async Task<IActionResult> UpdateEInvoiceHeader([FromBody] UpdateEInvoiceHeaderCommand command)
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
        public async Task<IActionResult> DeleteEInvoiceHeader(int id)
        {
            var result = await Mediator.Send(new DeleteEInvoiceHeaderCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "EInvoice Header deleted successfully." : "EInvoice Header not found."
            });
        }
    }
}
