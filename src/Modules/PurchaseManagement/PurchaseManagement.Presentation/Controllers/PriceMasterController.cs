using PurchaseManagement.Application.PriceMaster.Commands.Create;
using PurchaseManagement.Application.PriceMaster.Commands.Delete;
using PurchaseManagement.Application.PriceMaster.Commands.Update;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Application.PriceMaster.Queries.GetAll;
using PurchaseManagement.Application.PriceMaster.Queries.GetById;
using PurchaseManagement.Application.PriceMaster.Queries.GetPriceMasterPending;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public  class PriceMasterController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        public PriceMasterController(IMediator mediator) : base(mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PriceMasterCreateDto dto, CancellationToken ct)
        {
            var id = await _mediator.Send(new CreatePriceMasterCommand { Data = dto }, ct);
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = id
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] PriceMasterUpdateDto dto, CancellationToken ct)
        {
            // if (id != dto.Id) return BadRequest("Route id and body id mismatch.");
            var ok = await _mediator.Send(new UpdatePriceMasterCommand { Data = dto }, ct);
            if (!ok)
            {
                // Treat unsuccessful update as NotFound/Conflict per your handler semantics.
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"Price Master {dto.Id} not found or cannot be updated."
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = new { id = dto.Id },
                message = $"Price Master {dto.Id} updated successfully."
            });

        }


        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 15, [FromQuery] string? searchTerm = null,[FromQuery] int? itemId = null,
            [FromQuery] decimal? qtyFrom = null,
            [FromQuery] decimal? qtyTo   = null,[FromQuery] int? statusId   = null, [FromQuery] bool expiredList=false, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new GetAllPriceMasterQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                ItemId = itemId,
                QtyFrom = qtyFrom,
                QtyTo = qtyTo,
                statusId = statusId,
                expiredList = expiredList
            }, ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                page = result.Page,
                pageSize = result.PageSize,
                totalCount = result.Total,
                data = result.Items,
                message = result.Items.Count > 0
                    ? $"Fetched {result.Items.Count} record(s) out of {result.Total}."
                    : "No records found."
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetPriceMasterByIdQuery { Id = id }, ct);
            if (result == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"PriceMaster with ID {id} not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result,
                message = $"PriceMaster with ID {id} fetched successfully"
            });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _mediator.Send(new DeletePriceMasterCommand { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Deleted successfully.", errors = "" });

        }
         [HttpGet("pending")]
        public async Task<IActionResult> GetPendingAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 15,
             [FromQuery] string? searchTerm = null, CancellationToken ct = default)
        {
            var (rows, total) = await _mediator.Send(new GetPriceMasterPendingQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            }, ct);           
            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                data = new { rows , totalCount = total, pageNumber, pageSize, searchTerm },
                message = "Pending Price Master fetched successfully."
            });
        }  
    }
}
