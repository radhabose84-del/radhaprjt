using Contracts.Commands.Finance;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.Invoice.Commands.CreateInvoice;
using SalesManagement.Application.Invoice.Commands.UpdateInvoice;
using SalesManagement.Application.Invoice.Queries.GetAllInvoice;
using SalesManagement.Application.Invoice.Queries.GetInvoiceAutoComplete;
using SalesManagement.Application.Invoice.Queries.GetInvoiceById;
using SalesManagement.Application.Invoice.Queries.GetInvoiceGatePassPending;
using SalesManagement.Application.Invoice.Queries.GetInvoicePending;
using SalesManagement.Application.Invoice.Queries.GetInvoicePrintDetails;

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

        [HttpPut]
        public async Task<IActionResult> UpdateInvoice([FromBody] UpdateInvoiceCommand command)
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

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] string? searchTerm = null,
            CancellationToken ct = default)
        {
            var (rows, total) = await Mediator.Send(new GetInvoicePendingQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            }, ct);

            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                data = new { rows, totalCount = total, pageNumber, pageSize, searchTerm },
                message = "Pending Invoice details fetched successfully."
            });
        }

        [HttpGet("{id}/print")]
        public async Task<IActionResult> GetInvoicePrintDetailsAsync(int id)
        {
            var result = await Mediator.Send(new GetInvoicePrintDetailsQuery(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("gatepass-pending")]
        public async Task<IActionResult> GetGatePassPendingAsync(
            [FromQuery] string? vehicleNo = null,
            CancellationToken ct = default)
        {
            var result = await Mediator.Send(new GetInvoiceGatePassPendingQuery
            {
                VehicleNo = vehicleNo
            }, ct);

            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                data = result,
                message = "Gate Pass Pending Invoice details fetched successfully."
            });
        }

        /// <summary>
        /// Directly generates EInvoice (and optionally EWaybill) for a Sales Invoice
        /// by dispatching to the Finance module via Contracts command.
        /// </summary>
        [HttpPost("generate-einvoice")]
        public async Task<IActionResult> GenerateEInvoice(
            [FromQuery] int invoiceId,
            [FromQuery] bool withEInvoice = true,
            [FromQuery] bool withEwaybill = false)
        {
            if (!withEInvoice)
            {
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    isSuccess = true,
                    message = "withEInvoice is false — no EInvoice generation requested.",
                    data = (object?)null
                });
            }

            var result = await Mediator.Send(new CreateEInvoiceFromSalesCommand
            {
                InvoiceId = invoiceId,
                IsEwaybillCreate = withEwaybill
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }
    }
}
