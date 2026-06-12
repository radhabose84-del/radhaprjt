using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Arrival.Commands.CreateArrival;
using PurchaseManagement.Application.Arrival.Commands.DeleteArrival;
using PurchaseManagement.Application.Arrival.Commands.UpdateArrival;
using PurchaseManagement.Application.Arrival.Queries.GetAllArrival;
using PurchaseManagement.Application.Arrival.Queries.GetArrivalAutoComplete;
using PurchaseManagement.Application.Arrival.Queries.GetArrivalBalanceQty;
using PurchaseManagement.Application.Arrival.Queries.GetArrivalById;
using PurchaseManagement.Application.Arrival.Queries.GetArrivalLastLotNo;

namespace PurchaseManagement.Presentation.Controllers
{
    public class ArrivalController : ApiControllerBase
    {
        public ArrivalController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllArrivalAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] bool? PendingStatus = null,
            [FromQuery] int? StatusId = null,
            [FromQuery] DateTimeOffset? FromDate = null,
            [FromQuery] DateTimeOffset? ToDate = null)
        {
            var result = await Mediator.Send(new GetAllArrivalQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                PendingStatus = PendingStatus,
                StatusId = StatusId,
                FromDate = FromDate,
                ToDate = ToDate
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
        public async Task<IActionResult> GetArrivalByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetArrivalByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetArrivalAutoCompleteAsync([FromQuery] string term = "")
        {
            var result = await Mediator.Send(new GetArrivalAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        // Last lot number (latest arrival) for the current unit.
        [HttpGet("last-lotno")]
        public async Task<IActionResult> GetArrivalLastLotNoAsync()
        {
            var result = await Mediator.Send(new GetArrivalLastLotNoQuery());
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        // Remaining/balance qty per item for a Raw Material PO (unit-scoped): PO qty − arrived qty.
        [HttpGet("balance-qty")]
        public async Task<IActionResult> GetArrivalBalanceQtyAsync([FromQuery] int rawMaterialPoId)
        {
            var result = await Mediator.Send(new GetArrivalBalanceQtyQuery(rawMaterialPoId));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateArrival([FromBody] CreateArrivalCommand command)
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
        public async Task<IActionResult> UpdateArrival([FromBody] UpdateArrivalCommand command)
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
        public async Task<IActionResult> DeleteArrival(int id)
        {
            var result = await Mediator.Send(new DeleteArrivalCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Arrival deleted successfully." : "Arrival not found."
            });
        }
    }
}
