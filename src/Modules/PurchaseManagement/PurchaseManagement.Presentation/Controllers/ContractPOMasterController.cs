using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PurchaseManagement.Application.ContractPOMaster.Commands.Create;
using PurchaseManagement.Application.ContractPOMaster.Commands.Update;
using PurchaseManagement.Application.ContractPOMaster.Commands.Delete;
using PurchaseManagement.Application.ContractPOMaster.Queries.GetAll;
using PurchaseManagement.Application.ContractPOMaster.Queries.GetById;
using PurchaseManagement.Application.ContractPOMaster.Queries.AutoComplete;
using PurchaseManagement.Application.ContractPOMaster.Queries.GetPending;

namespace PurchaseManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ContractPOMasterController : ApiControllerBase
    {
        public ContractPOMasterController(ISender mediator) : base(mediator)
        {
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateContractPOMasterCommand command, CancellationToken ct = default)
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
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateContractPOMasterCommand command, CancellationToken ct = default)
        {
            var result = await Mediator.Send(command, ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Updated successfully.",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id, CancellationToken ct = default)
        {
            var result = await Mediator.Send(new DeleteContractPOMasterCommand(id), ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result ? "Deleted successfully." : "Delete failed.",
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
            var result = await Mediator.Send(new GetAllContractPOMasterQuery(pageNumber, pageSize, search), ct);

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
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id, CancellationToken ct = default)
        {
            var data = await Mediator.Send(new GetContractPOMasterByIdQuery(id), ct);

            return Ok(new
            {
                StatusCode = data is null ? StatusCodes.Status404NotFound : StatusCodes.Status200OK,
                message = data is null ? "Not found" : "Fetched",
                data
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> AutoCompleteAsync([FromQuery] string? term = null, CancellationToken ct = default)
        {
            var data = await Mediator.Send(new GetContractPOMasterAutoCompleteQuery(term ?? string.Empty), ct);

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
            var (items, total) = await Mediator.Send(new GetContractPOMasterPendingQuery
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
