using PurchaseManagement.Application.DutyMaster.Command.Create;
using PurchaseManagement.Application.DutyMaster.Delete;
using PurchaseManagement.Application.DutyMaster;
using PurchaseManagement.Application.DutyMaster.Queries.GetDutyMasterById;
using PurchaseManagement.Application.Purchase.DutyMaster.Command.Update;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.DutyMaster.Queries.GetDutyMasterAutocomplete;
using PurchaseManagement.Application.DutyMaster.Queries.GetAllDutyMaster;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DutyMasterController : ControllerBase
    {
        private readonly IMediator _mediator;
        public DutyMasterController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var (items, total) = await _mediator.Send(new GetAllDutyMasterQuery(pageNumber, pageSize, search), ct);
            var data = new { items, total };

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Fetched",
                data
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var dto = await _mediator.Send(new GetDutyMasterByIdQuery(id), ct);

            return Ok(new
            {
                StatusCode = dto is null ? StatusCodes.Status404NotFound : StatusCodes.Status200OK,
                message = dto is null ? "Not found" : "Fetched",
                data = dto
            });
        }

        // POST: api/DutyMaster
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] DutyMasterDto model, CancellationToken ct = default)
        {
            var id = await _mediator.Send(new CreateDutyMasterCommand { Model = model }, ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = id
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] DutyMasterDto dto, CancellationToken ct = default)
        {
            var ok = await _mediator.Send(new UpdateDutyMasterCommand
            {
                Model = dto
            }, ct);

            return Ok(new
            {
                StatusCode = ok ? StatusCodes.Status200OK : StatusCodes.Status404NotFound,
                message = ok ? "Updated successfully." : "Not found.",
                data = ok
            });

        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct = default)
        {
            await _mediator.Send(new DeleteDutyMasterCommand(id), ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Deleted successfully.",
                data = true
            });
        }
        [HttpGet("autocomplete")]
        public async Task<IActionResult> Autocomplete([FromQuery] string? term, CancellationToken ct = default)
        {
            var items = await _mediator.Send(new GetDutyMasterAutocompleteQuery(term), ct);
            return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Fetched", data = items });
        }
    }
}
