using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.CancelPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.CreatePurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.DeletePurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.SubmitPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.UpdatePurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetAllPurchaseReturns;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnAutoComplete;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnById;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetReturnableQtyByGrn;

namespace PurchaseManagement.Presentation.Controllers;

[Route("api/[controller]")]
public class PurchaseReturnController : ApiControllerBase
{
    private readonly IValidator<CreatePurchaseReturnCommand> _createValidator;
    private readonly IValidator<UpdatePurchaseReturnCommand> _updateValidator;
    private readonly IValidator<DeletePurchaseReturnCommand> _deleteValidator;

    public PurchaseReturnController(
        ISender mediator,
        IValidator<CreatePurchaseReturnCommand> createValidator,
        IValidator<UpdatePurchaseReturnCommand> updateValidator,
        IValidator<DeletePurchaseReturnCommand> deleteValidator) : base(mediator)
    {
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _deleteValidator = deleteValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int PageNumber = 1,
        [FromQuery] int PageSize = 20,
        [FromQuery] string? SearchTerm = null)
    {
        var result = await Mediator.Send(new GetAllPurchaseReturnsQuery(PageNumber, PageSize, SearchTerm));

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            data = result.Items,
            TotalCount = result.Total,
            PageNumber,
            PageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await Mediator.Send(new GetPurchaseReturnByIdQuery(id));
        return Ok(new { StatusCode = StatusCodes.Status200OK, data });
    }

    [HttpGet("autocomplete")]
    public async Task<IActionResult> AutoComplete([FromQuery] string? term = null)
    {
        var data = await Mediator.Send(new GetPurchaseReturnAutoCompleteQuery(term));
        return Ok(new { StatusCode = StatusCodes.Status200OK, data });
    }

    [HttpGet("returnable-qty")]
    public async Task<IActionResult> GetReturnableQty([FromQuery] int grnHeaderId)
    {
        var data = await Mediator.Send(new GetReturnableQtyByGrnQuery(grnHeaderId));
        return Ok(new { StatusCode = StatusCodes.Status200OK, data });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePurchaseReturnCommand command)
    {
        var validationResult = await _createValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = "Validation failed",
                errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
            });
        }

        var response = await Mediator.Send(command);
        return Ok(new
        {
            StatusCode = StatusCodes.Status201Created,
            message = "Purchase Return created successfully.",
            data = response
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdatePurchaseReturnCommand command)
    {
        if (id != command.Id)
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = "Route id and body id do not match."
            });

        var validationResult = await _updateValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = "Validation failed",
                errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
            });
        }

        var response = await Mediator.Send(command);
        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Purchase Return updated successfully.",
            data = response
        });
    }

    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        await Mediator.Send(new SubmitPurchaseReturnCommand(id));
        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Purchase Return submitted for approval."
        });
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        await Mediator.Send(new CancelPurchaseReturnCommand(id));
        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Purchase Return cancelled."
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeletePurchaseReturnCommand(id);
        var validationResult = await _deleteValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = "Validation failed",
                errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
            });
        }

        await Mediator.Send(command);
        return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Purchase Return deleted successfully." });
    }
}
