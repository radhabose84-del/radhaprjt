using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.ProformaInvoice.Commands.CreateProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaPayment;
using SalesManagement.Application.ProformaInvoice.Commands.DeleteProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Queries.GetAllProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceById;
using SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceAutoComplete;
using SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceBySalesOrder;
using SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoicePrintDetails;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ProformaInvoiceController : ApiControllerBase
    {
        public ProformaInvoiceController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllProformaInvoiceAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllProformaInvoiceQuery
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
        public async Task<IActionResult> GetProformaInvoiceByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetProformaInvoiceByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetProformaInvoiceAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetProformaInvoiceAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-sales-order/{salesOrderId}")]
        public async Task<IActionResult> GetProformaInvoiceBySalesOrderAsync(int salesOrderId)
        {
            var result = await Mediator.Send(new GetProformaInvoiceBySalesOrderQuery { SalesOrderId = salesOrderId });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateProformaInvoice([FromBody] CreateProformaInvoiceCommand command)
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
        public async Task<IActionResult> UpdateProformaInvoice([FromBody] UpdateProformaInvoiceCommand command)
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

        [HttpPut("update-payment")]
        public async Task<IActionResult> UpdateProformaPayment([FromBody] UpdateProformaPaymentCommand command)
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

        [HttpGet("{id}/print")]
        public async Task<IActionResult> GetProformaInvoicePrintDetailsAsync(int id)
        {
            var result = await Mediator.Send(new GetProformaInvoicePrintDetailsQuery(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProformaInvoice(int id)
        {
            var result = await Mediator.Send(new DeleteProformaInvoiceCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = "Proforma Invoice deleted successfully."
            });
        }
    }
}
