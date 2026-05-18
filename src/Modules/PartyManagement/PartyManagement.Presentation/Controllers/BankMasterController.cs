using PartyManagement.Application.BankMaster.Command.Create;
using PartyManagement.Application.BankMaster.Command.Delete;
using PartyManagement.Application.BankMaster.Command.Update;
using PartyManagement.Application.BankMaster.Queries.GetBankMasterById;
using PartyManagement.Application.BankMaster.Queries.GetBankMastersAutocomplete;
using PartyManagement.Application.BankMaster.Queries.GetBankMastersPaged;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PartyManagement.Presentation.Controllers;

[Route("api/[controller]")]
public class BankMasterController : ApiControllerBase
{
    private readonly ISender _mediator;
    public BankMasterController(ISender mediator) : base(mediator) => _mediator = mediator;

    // GET: api/BankMaster
    [HttpGet]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize   = 20,
        [FromQuery] string? searchTerm = null,
        CancellationToken ct = default)
    {
        pageNumber = pageNumber <= 0 ? 1  : pageNumber;
        pageSize   = pageSize   <= 0 ? 20 : pageSize;

        var (items, total) = await _mediator.Send(
            new GetBankMastersPagedQuery(
                PageNumber: pageNumber,
                PageSize: pageSize,
                Search: searchTerm
            ), ct);

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            data = items,
            TotalCount = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    // GET: api/BankMaster/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await _mediator.Send(new GetBankMasterByIdQuery(id), ct);
        if (dto is null)
            return NotFound(new
            {
                StatusCode = StatusCodes.Status404NotFound,
                message = "Bank not found."
            });

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "ID fetched successfully",
            data = dto
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateBankMasterCommand body, CancellationToken ct)
    {
        var id = await _mediator.Send(body, ct);
        return StatusCode(StatusCodes.Status201Created, new
        {
            StatusCode = StatusCodes.Status201Created,
            message = "Created successfully.",
            data = new { Id = id }
        });
    }
    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateBankMasterCommand body, CancellationToken ct)
    {
        await _mediator.Send(body, ct);
        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Updated successfully.",
            data = true
        });
    }

    // DELETE: api/BankMaster/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteBankMasterCommand(id), ct);
        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Deleted successfully.",
            data = true
        });
    }

    // GET: api/BankMaster/by-name?term=icici
    [HttpGet("by-name")]
    public async Task<IActionResult> GetAllAutocompleteAsync([FromQuery] string? term, CancellationToken ct = default)
    {
        var data = await _mediator.Send(new GetBankMastersAutocompleteQuery(term), ct);
        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Success",
            errors = "",
            data
        });
    }
}
