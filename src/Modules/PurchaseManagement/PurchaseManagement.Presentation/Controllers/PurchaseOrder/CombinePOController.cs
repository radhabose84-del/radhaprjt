using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PurchaseOrder.CombinePO;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Create.Command;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Amendment;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Queries.GetCombinePOById;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Commands.Update;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Command.Cancel;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Command.Foreclose;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Queries.GetCombinePOPending;
using PurchaseManagement.Application.PurchaseOrder.EmergencyPO.Queries.GetEmergencyPOPending;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Presentation.Controllers.PurchaseOrder
{
    [Route("api/[controller]")]
    public sealed class CombinePOController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        public CombinePOController(IMediator mediator) : base(mediator) => _mediator = mediator;
    
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCombinePODto dto, CancellationToken ct)
        {
            var response = await _mediator.Send(new CreateCombinePOCommand(dto), ct);

            return Ok(new
            {
                StatusCode = response.IsSuccess
                    ? StatusCodes.Status201Created
                    : StatusCodes.Status400BadRequest,   
                message = response.Message,
                data = response.Data
            });
        }

        
        [HttpPost("amendment")]
        public async Task<IActionResult> Amend([FromBody] AmendCombinePODto dto, CancellationToken ct)
        {
            var newId = await _mediator.Send(new AmendCombinePOCommand(dto), ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Amendment created successfully.",
                data = new { NewPurchaseOrderId = newId }
            });
        }
        [HttpGet("pending-po")]
        public async Task<IActionResult> GetPendingPOAsync(
            [FromQuery] int? poId = null,
            [FromQuery] int? poMethodId = null,
            [FromQuery] int? pageNumber = 1,
            [FromQuery] int? pageSize = 15,
            [FromQuery] string? searchTerm = null,
            CancellationToken ct = default)
        {
            var result = await _mediator.Send(new GetCombinePOPendingQuery
            {
                PoId = poId,
                PoMethodId = poMethodId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            }, ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result,
                message = "Pending PO data fetched successfully"
            });
        }

        [HttpGet("emergency-pending-po")]
        public async Task<IActionResult> GetEmergencyPendingPOAsync(
            [FromQuery] int? poId = null,
            [FromQuery] int? poMethodId = null,
            [FromQuery] int? pageNumber = 1,
            [FromQuery] int? pageSize = 15,
            [FromQuery] string? searchTerm = null,
            CancellationToken ct = default)
        {
            var (items, totalCount) = await _mediator.Send(new GetEmergencyPOPendingQuery
            {
                PoId = poId,
                PoMethodId = poMethodId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            }, ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                message = "Emergency pending PO data fetched successfully"
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] int poMethodId, CancellationToken ct)
        {
            var vm = await _mediator.Send(new GetCombinePOByIdQuery(id, poMethodId), ct);
            return Ok(vm);
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCombinePODto dto, CancellationToken ct)
        {
            var ok = await _mediator.Send(new UpdateCombinePOCommand(dto), ct);
            return Ok(new
            {
                StatusCode = ok ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest,
                message = ok ? "Updated successfully." : "Update failed.",
                data = ok
            });
        }

        [HttpPut("cancel/{id:int}")]
        public async Task<IActionResult> Cancel([FromRoute] int id, [FromQuery] int poMethodId, CancellationToken ct)
        {
            var result = await _mediator.Send(new CancelCombinePOCommand(id, poMethodId), ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Purchase Order cancelled successfully." : "Failed to cancel Purchase Order."
            });
        }

        [HttpPut("foreclose/{id:int}")]
        public async Task<IActionResult> Foreclose([FromRoute] int id, [FromQuery] int poMethodId, CancellationToken ct)
        {
            var result = await _mediator.Send(new ForecloseCombinePOCommand(id, poMethodId), ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Purchase Order foreclosed successfully." : "Failed to foreclose Purchase Order."
            });
        }
    }
}