using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PurchaseOrder.CombinePO;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Command;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Amendment;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Queries.GetCombinePOById;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Commands.Update;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.API.Controllers.PurchaseOrder
{    
    [ApiController]
    [Route("api/[controller]")]
    public sealed class CombinePOController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CombinePOController(IMediator mediator) => _mediator = mediator;
    
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
    }
}