using PartyManagement.Application.BankAccount.Command.CreateBankAccount;
using PartyManagement.Application.BankAccount.Command.DeleteBankAccount;
using PartyManagement.Application.BankAccount.Command.UpdateBankAccount;
using PartyManagement.Application.BankAccount.Query.GetBankAccountById;
using PartyManagement.Application.BankAccount.Query.GetBankAccountsPaged;
using PartyManagement.Application.BankAccount.Query.GetBankAutocomplete;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;


namespace PartyManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankAccountController : ControllerBase
{
    private readonly ISender _mediator;
    public BankAccountController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAllBankAccountsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,[FromQuery] int? bankId = null)
    {
        // normalize
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 20 : pageSize;

        var (items, total) = await _mediator.Send(
            new GetAllBankAccountsQuery(
                PageNumber: pageNumber,
                PageSize: pageSize,
                Search: searchTerm,
                BankId: bankId
            )
        );
        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            data = items,
            TotalCount = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _mediator.Send(new GetBankAccountByIdQuery(id));
        if (dto is null)
            return NotFound(new
            {
                StatusCode = StatusCodes.Status404NotFound,
                message = "Bank account not found."
            });

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "ID fetched successfully",
            data = dto
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBankAccountCommand body)
    {

        var id = await _mediator.Send(body);
        return StatusCode(StatusCodes.Status201Created, new
        {
            StatusCode = StatusCodes.Status201Created,
            message = "Created successfully.",
            data = new { Id = id }
        });

    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateBankAccountCommand body)
    {
        var ok = await _mediator.Send(body);
        if (!ok)
            return NotFound(new
            {
                StatusCode = StatusCodes.Status404NotFound,
                message = "Bank account not found."
            });

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Updated successfully.",
            data = true
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _mediator.Send(new DeleteBankAccountCommand(id));
        if (!ok)
            return NotFound(new
            {
                StatusCode = StatusCodes.Status404NotFound,
                message = "Bank account not found."
            });

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Deleted successfully.",
            data = true
        });
    }
    [HttpGet("by-name")]
    public async Task<IActionResult> GetAllAutocomplete([FromQuery] string? term, CancellationToken ct)
    {
        var data = await _mediator.Send(new GetBankAccountAutoCompleteQuery(term), ct);

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Success",
            errors = "",
            data
        });
    }
    
}