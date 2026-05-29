using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Cancel;
using PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Create;
using PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Foreclose;
using PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Update;
using PurchaseManagement.Application.PurchaseOrder.BlanketPO.Queries.GetById;
using PurchaseManagement.Application.PurchaseOrder.BlanketPO.Queries.GetPending;
using PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;

namespace PurchaseManagement.Presentation.Controllers.PurchaseOrder
{
    [Route("api/[controller]")]
    public class BlanketPOController : ApiControllerBase
    {
        public BlanketPOController(ISender mediator) : base(mediator)
        {
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(
            [FromBody] BlanketPOCreateDto dto, CancellationToken ct = default)
        {
            var response = await Mediator.Send(new CreateBlanketPOCommand(dto), ct);

            return Ok(new
            {
                StatusCode = response.IsSuccess
                    ? StatusCodes.Status201Created
                    : StatusCodes.Status400BadRequest,
                message = response.Message,
                data = response.Data
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(
            [FromBody] BlanketPOUpdateDto dto, CancellationToken ct = default)
        {
            var ok = await Mediator.Send(new UpdateBlanketPOCommand(dto), ct);

            return Ok(new
            {
                StatusCode = ok ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest,
                message = ok ? "Updated successfully." : "Update failed.",
                data = ok
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] int id, CancellationToken ct = default)
        {
            var data = await Mediator.Send(new GetBlanketPOByIdQuery(id), ct);

            return Ok(new
            {
                StatusCode = data is null ? StatusCodes.Status404NotFound : StatusCodes.Status200OK,
                message = data is null ? "Not found" : "Fetched",
                data
            });
        }

        [HttpPut("cancel/{id:int}")]
        public async Task<IActionResult> Cancel(
            [FromRoute] int id, CancellationToken ct)
        {
            var result = await Mediator.Send(new CancelBlanketPOCommand(id), ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Blanket PO cancelled successfully." : "Failed to cancel Blanket PO."
            });
        }

        [HttpPut("foreclose/{id:int}")]
        public async Task<IActionResult> Foreclose(
            [FromRoute] int id, CancellationToken ct)
        {
            var result = await Mediator.Send(new ForecloseBlanketPOCommand(id), ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Blanket PO foreclosed successfully." : "Failed to foreclose Blanket PO."
            });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingAsync(
            [FromQuery] int? poId = null,
            [FromQuery] int? pageNumber = 1,
            [FromQuery] int? pageSize = 15,
            [FromQuery] string? searchTerm = null,
            CancellationToken ct = default)
        {
            var (items, total) = await Mediator.Send(new GetBlanketPOPendingQuery
            {
                PoId = poId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            }, ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
    }
}
