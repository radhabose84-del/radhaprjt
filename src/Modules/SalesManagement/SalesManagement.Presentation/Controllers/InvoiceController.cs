using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.Invoice.Commands.CreateInvoice;
using SalesManagement.Application.Invoice.Commands.DeleteInvoice;
using SalesManagement.Application.Invoice.Queries.GetAllInvoice;
using SalesManagement.Application.Invoice.Queries.GetInvoiceAutoComplete;
using SalesManagement.Application.Invoice.Queries.GetInvoiceByDispatchAdvice;
using SalesManagement.Application.Invoice.Queries.GetInvoiceById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class InvoiceController : ApiControllerBase
    {
        public InvoiceController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllInvoiceAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllInvoiceQuery
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
        public async Task<IActionResult> GetInvoiceByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetInvoiceByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetInvoiceAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetInvoiceAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-dispatch/{dispatchAdviceId}")]
        public async Task<IActionResult> GetInvoiceByDispatchAdviceAsync(int dispatchAdviceId)
        {
            var result = await Mediator.Send(new GetInvoiceByDispatchAdviceQuery
            {
                DispatchAdviceId = dispatchAdviceId
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceCommand command)
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
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var result = await Mediator.Send(new DeleteInvoiceCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = "Invoice deleted successfully."
            });
        }
    }
}
