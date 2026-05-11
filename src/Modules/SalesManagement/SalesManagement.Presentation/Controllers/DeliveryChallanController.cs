using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.DeliveryChallan.Commands.CreateDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Commands.DeleteDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Commands.GenerateEWaybillForDC;
using SalesManagement.Application.DeliveryChallan.Queries.GetStandaloneEwbDocument;
using SalesManagement.Application.DeliveryChallan.Queries.GetAllDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanById;
using SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanAutoComplete;
using SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanForReceipt;
using SalesManagement.Application.DeliveryChallan.Queries.GetPendingDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Queries.GetPendingDeliveryChallanById;
using SalesManagement.Application.DeliveryChallan.Queries.GetDCGatePassPending;
using SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanPrintDetails;
using SalesManagement.Application.DeliveryChallan.Queries.GetStoOpenQty;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class DeliveryChallanController : ApiControllerBase
    {
        public DeliveryChallanController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllDeliveryChallanAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllDeliveryChallanQuery
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

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingDeliveryChallanAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetPendingDeliveryChallanQuery
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
        public async Task<IActionResult> GetPendingDeliveryChallanByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetPendingDeliveryChallanByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("{id}/print")]
        public async Task<IActionResult> GetDeliveryChallanPrintDetailsAsync(int id)
        {
            var result = await Mediator.Send(new GetDeliveryChallanPrintDetailsQuery(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeliveryChallanByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetDeliveryChallanByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetDeliveryChallanAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetDeliveryChallanAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("for-receipt")]
        public async Task<IActionResult> GetDeliveryChallanForReceiptAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetDeliveryChallanForReceiptQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("sto-open-qty")]
        public async Task<IActionResult> GetStoOpenQtyAsync([FromQuery] int stoDetailId)
        {
            var result = await Mediator.Send(new GetStoOpenQtyQuery { StoDetailId = stoDetailId });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeliveryChallan([FromBody] CreateDeliveryChallanCommand command)
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

        [HttpGet("gatepass-pending")]
        public async Task<IActionResult> GetDCGatePassPendingAsync(
            [FromQuery] string? vehicleNo = null,
            CancellationToken ct = default)
        {
            var result = await Mediator.Send(new GetDCGatePassPendingQuery
            {
                VehicleNo = vehicleNo
            }, ct);

            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                data = result,
                message = "DC Gate Pass Pending details fetched successfully."
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDeliveryChallan(int id)
        {
            var result = await Mediator.Send(new DeleteDeliveryChallanCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = "Delivery Challan deleted successfully."
            });
        }

        /// <summary>
        /// Generates an e-waybill header for the given Delivery Challan.
        /// Idempotent — if an e-waybill already exists for this DC the existing record
        /// is returned with AlreadyExisted = true. Frontend can call this safely on retry.
        /// </summary>
        [HttpPost("{id}/generate-ewaybill")]
        public async Task<IActionResult> GenerateEWaybillAsync(int id)
        {
            var result = await Mediator.Send(new GenerateEWaybillForDCCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Returns the print-ready EWB document view (consignor/consignee/items/totals/transporter)
        /// for the latest standalone e-Waybill linked to this DC. Used to render the "Print EWB" page.
        /// </summary>
        [HttpGet("{id}/ewaybill-document")]
        public async Task<IActionResult> GetEwbDocumentAsync(int id)
        {
            var result = await Mediator.Send(new GetStandaloneEwbDocumentQuery(id));

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
