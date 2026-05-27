using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.CreateReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.DeleteReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.UpdateReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetAllReturnReasons;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonAutoComplete;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonById;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonsByReturnType;

namespace PurchaseManagement.Presentation.Controllers;

[Route("api/[controller]")]
public class ReturnReasonController : ApiControllerBase
{
    private readonly IValidator<CreateReturnReasonCommand> _createValidator;
    private readonly IValidator<UpdateReturnReasonCommand> _updateValidator;
    private readonly IValidator<DeleteReturnReasonCommand> _deleteValidator;

    public ReturnReasonController(
        ISender mediator,
        IValidator<CreateReturnReasonCommand> createValidator,
        IValidator<UpdateReturnReasonCommand> updateValidator,
        IValidator<DeleteReturnReasonCommand> deleteValidator) : base(mediator)
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
        var result = await Mediator.Send(new GetAllReturnReasonsQuery(PageNumber, PageSize, SearchTerm));

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            data = result.Items,
            TotalCount = result.Total,
            PageNumber,
            PageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await Mediator.Send(new GetReturnReasonByIdQuery(id));
        return Ok(new { StatusCode = StatusCodes.Status200OK, data });
    }

    [HttpGet("by-name")]
    public async Task<IActionResult> AutoComplete([FromQuery] string? term = null)
    {
        var data = await Mediator.Send(new GetReturnReasonAutoCompleteQuery(term));
        return Ok(new { StatusCode = StatusCodes.Status200OK, data });
    }

    [HttpGet("by-return-type/{returnTypeId:int}")]
    public async Task<IActionResult> GetByReturnType(int returnTypeId)
    {
        var data = await Mediator.Send(new GetReturnReasonsByReturnTypeQuery(returnTypeId));
        return Ok(new { StatusCode = StatusCodes.Status200OK, data });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReturnReasonCommand command)
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
            message = "Return Reason created successfully.",
            data = response
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateReturnReasonCommand command)
    {
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
            message = "Return Reason updated successfully.",
            data = response
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteReturnReasonCommand(id);
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
        return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Return Reason deleted successfully." });
    }
}
