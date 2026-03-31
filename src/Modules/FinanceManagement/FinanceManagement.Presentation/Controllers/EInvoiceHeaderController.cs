using Contracts.Commands.Finance;
using FinanceManagement.Application.EInvoiceHeader.Commands.CancelEwb;
using FinanceManagement.Application.EInvoiceHeader.Commands.CancelIrn;
using FinanceManagement.Application.EInvoiceHeader.Commands.CreateEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.DeleteEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.GenerateEwb;
using FinanceManagement.Application.EInvoiceHeader.Commands.GenerateIrn;
using FinanceManagement.Application.EInvoiceHeader.Commands.UpdateEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetAllEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetEInvoiceHeaderAutoComplete;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetEInvoiceHeaderById;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetEwbDetails;
using FinanceManagement.Application.EInvoiceHeader.Queries.GetIrnDetails;
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

        /// <summary>
        /// Generates IRN from NIC API. Optionally generates e-Waybill together
        /// when transport details (Distance, VehNo, etc.) are provided in the body.
        /// Case 1: POST with transport details → IRN + e-Waybill in one call.
        /// Case 2: POST without transport details → IRN only.
        /// </summary>
        [HttpPost("generate-irn")]
        public async Task<IActionResult> GenerateIrn([FromBody] GenerateIrnCommand command)
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

        /// <summary>
        /// Generates an e-Waybill from an existing IRN by calling the NIC e-Waybill API
        /// with transport details.
        /// </summary>
        [HttpPost("generate-ewb")]
        public async Task<IActionResult> GenerateEwb([FromBody] GenerateEwbCommand command)
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

        /// <summary>
        /// Cancels an IRN within 24 hours of generation.
        /// CnlRsn: "1"=Duplicate, "2"=Data entry mistake, "3"=Order cancelled, "4"=Others
        /// </summary>
        [HttpPost("cancel-irn")]
        public async Task<IActionResult> CancelIrn([FromBody] CancelIrnCommand command)
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

        /// <summary>
        /// Cancels an e-Waybill within 24 hours. Must cancel EWB before cancelling IRN.
        /// CancelRsnCode: 1=Duplicate, 2=Data entry mistake, 3=Order cancelled, 4=Others
        /// </summary>
        [HttpPost("cancel-ewb")]
        public async Task<IActionResult> CancelEwb([FromBody] CancelEwbCommand command)
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

        /// <summary>
        /// Fetches IRN details from NIC API for the given EInvoiceHeader.
        /// </summary>
        [HttpGet("irn-details/{id}")]
        public async Task<IActionResult> GetIrnDetails(int id)
        {
            var result = await Mediator.Send(new GetIrnDetailsQuery { EInvoiceHeaderId = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Fetches e-Waybill details from NIC API by IRN for the given EInvoiceHeader.
        /// </summary>
        [HttpGet("ewb-details/{id}")]
        public async Task<IActionResult> GetEwbDetails(int id)
        {
            var result = await Mediator.Send(new GetEwbDetailsQuery { EInvoiceHeaderId = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Creates an EInvoice from a Sales Invoice by fetching data via shared lookup,
        /// then generates IRN and optionally e-Waybill.
        /// </summary>
        [HttpPost("create-from-sales")]
        public async Task<IActionResult> CreateEInvoiceFromSales(
            [FromBody] CreateEInvoiceFromSalesCommand command)
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
    }
}
