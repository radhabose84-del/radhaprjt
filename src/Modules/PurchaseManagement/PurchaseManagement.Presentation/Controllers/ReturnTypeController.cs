using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.CreateReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.DeleteReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.UpdateReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetAllReturnTypes;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetReturnTypeAutoComplete;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetReturnTypeById;

namespace PurchaseManagement.Presentation.Controllers;

[Route("api/[controller]")]
public class ReturnTypeController : ApiControllerBase
{
    private readonly IValidator<CreateReturnTypeCommand> _createValidator;
    private readonly IValidator<UpdateReturnTypeCommand> _updateValidator;
    private readonly IValidator<DeleteReturnTypeCommand> _deleteValidator;

    public ReturnTypeController(
        ISender mediator,
        IValidator<CreateReturnTypeCommand> createValidator,
        IValidator<UpdateReturnTypeCommand> updateValidator,
        IValidator<DeleteReturnTypeCommand> deleteValidator) : base(mediator)
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
        var result = await Mediator.Send(new GetAllReturnTypesQuery(PageNumber, PageSize, SearchTerm));

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
        var data = await Mediator.Send(new GetReturnTypeByIdQuery(id));
        return Ok(new { StatusCode = StatusCodes.Status200OK, data });
    }

    [HttpGet("by-name")]
    public async Task<IActionResult> AutoComplete([FromQuery] string? term = null)
    {
        var data = await Mediator.Send(new GetReturnTypeAutoCompleteQuery(term));
        return Ok(new { StatusCode = StatusCodes.Status200OK, data });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReturnTypeCommand command)
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
            message = "Return Type created successfully.",
            data = response
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateReturnTypeCommand command)
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
            message = "Return Type updated successfully.",
            data = response
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteReturnTypeCommand(id);
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
        return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Return Type deleted successfully." });
    }
}
