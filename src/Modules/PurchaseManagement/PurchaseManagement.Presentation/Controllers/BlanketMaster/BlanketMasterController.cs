using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.BlanketMaster.Commands.Create;
using PurchaseManagement.Application.BlanketMaster.Commands.Update;
using PurchaseManagement.Application.BlanketMaster.Queries.AutoComplete;
using PurchaseManagement.Application.BlanketMaster.Queries.GetAll;
using PurchaseManagement.Application.BlanketMaster.Queries.GetById;
using PurchaseManagement.Application.BlanketMaster.Queries.GetPending;

namespace PurchaseManagement.Presentation.Controllers.BlanketMaster
{
    [Route("api/[controller]")]
    public class BlanketMasterController : ApiControllerBase
    {
        public BlanketMasterController(ISender mediator) : base(mediator)
        {
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(
            [FromBody] CreateBlanketMasterCommand command, CancellationToken ct = default)
        {
            var result = await Mediator.Send(command, ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = result
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(
            [FromBody] UpdateBlanketMasterCommand command, CancellationToken ct = default)
        {
            var result = await Mediator.Send(command, ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Updated successfully.",
                data = result
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var result = await Mediator.Send(
                new GetAllBlanketMasterQuery(pageNumber, pageSize, search), ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Items,
                TotalCount = result.Total,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] int id, CancellationToken ct = default)
        {
            var data = await Mediator.Send(new GetBlanketMasterByIdQuery(id), ct);

            return Ok(new
            {
                StatusCode = data is null ? StatusCodes.Status404NotFound : StatusCodes.Status200OK,
                message = data is null ? "Not found" : "Fetched",
                data
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> AutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] bool approved = true,
            [FromQuery] int? vendorId = null,
            [FromQuery] DateTimeOffset? poDate = null,
            CancellationToken ct = default)
        {
            var data = await Mediator.Send(
                new GetBlanketMasterAutoCompleteQuery(
                    term ?? string.Empty, approved, vendorId, poDate), ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Fetched",
                data
            });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingAsync(
            [FromQuery] int? pageNumber = 1,
            [FromQuery] int? pageSize = 15,
            [FromQuery] string? searchTerm = null,
            CancellationToken ct = default)
        {
            var (items, total) = await Mediator.Send(new GetBlanketMasterPendingQuery
            {
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
