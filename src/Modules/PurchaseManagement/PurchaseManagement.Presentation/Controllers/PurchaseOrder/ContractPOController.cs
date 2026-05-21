using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Create;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Update;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Delete;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Amendment;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Queries.GetById;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Queries.GetContractPOPending;

namespace PurchaseManagement.Presentation.Controllers.PurchaseOrder
{
    [Route("api/[controller]")]
    public class ContractPOController : ApiControllerBase
    {
        public ContractPOController(ISender mediator) : base(mediator)
        {
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ContractPOCreateDto dto, CancellationToken ct = default)
        {
            var response = await Mediator.Send(new CreateContractPOCommand(dto), ct);

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
        public async Task<IActionResult> UpdateAsync([FromBody] ContractPOUpdateDto dto, CancellationToken ct = default)
        {
            var ok = await Mediator.Send(new UpdateContractPOCommand(dto), ct);

            return Ok(new
            {
                StatusCode = ok ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest,
                message = ok ? "Updated successfully." : "Update failed.",
                data = ok
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id, CancellationToken ct = default)
        {
            var result = await Mediator.Send(new DeleteContractPOCommand(id), ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result ? "Deleted successfully." : "Delete failed.",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id, CancellationToken ct = default)
        {
            var data = await Mediator.Send(new GetContractPOByIdQuery(id), ct);

            return Ok(new
            {
                StatusCode = data is null ? StatusCodes.Status404NotFound : StatusCodes.Status200OK,
                message = data is null ? "Not found" : "Fetched",
                data
            });
        }

        [HttpPost("amendment")]
        public async Task<IActionResult> AmendAsync([FromBody] ContractPOUpdateDto dto, CancellationToken ct = default)
        {
            var newId = await Mediator.Send(new ContractPOAmendmentCommand { Data = dto }, ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Amendment created successfully.",
                data = new { NewPurchaseOrderId = newId }
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
            var (items, total) = await Mediator.Send(new GetContractPOPendingQuery
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
