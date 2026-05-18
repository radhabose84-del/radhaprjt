using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.ContractPO.Commands.Create;
using PurchaseManagement.Application.ContractPO.Commands.Delete;
using PurchaseManagement.Application.ContractPO.Commands.Update;
using PurchaseManagement.Application.ContractPO.Queries.AutoComplete;
using PurchaseManagement.Application.ContractPO.Queries.GetAll;
using PurchaseManagement.Application.ContractPO.Queries.GetById;

namespace PurchaseManagement.Presentation.Controllers;

[Route("api/[controller]")]
public class ContractPOController : ApiControllerBase
{
    private readonly IValidator<CreateContractPOCommand> _createValidator;
    private readonly IValidator<UpdateContractPOCommand> _updateValidator;
    private readonly IValidator<DeleteContractPOCommand> _deleteValidator;

    public ContractPOController(
        ISender mediator,
        IValidator<CreateContractPOCommand> createValidator,
        IValidator<UpdateContractPOCommand> updateValidator,
        IValidator<DeleteContractPOCommand> deleteValidator
    ) : base(mediator)
    {
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _deleteValidator = deleteValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int PageNumber,
        [FromQuery] int PageSize,
        [FromQuery] string? SearchTerm = null)
    {
        var result = await Mediator.Send(
            new GetAllContractPOQuery(PageNumber, PageSize, SearchTerm));

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
    [ActionName(nameof(GetByIdAsync))]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var data = await Mediator.Send(new GetContractPOByIdQuery(id));

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            data
        });
    }

    [HttpGet("by-name")]
    public async Task<IActionResult> AutoComplete([FromQuery] string? name)
    {
        var data = await Mediator.Send(
            new GetContractPOAutoCompleteQuery(name ?? string.Empty));

        return Ok(new { StatusCode = StatusCodes.Status200OK, data });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateContractPOCommand command)
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
            message = "Created Successfully",
            errors = "",
            data = response
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateContractPOCommand command)
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
            message = "Updated Successfully",
            errors = "",
            data = response
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleteCommand = new DeleteContractPOCommand(id);
        var validationResult = await _deleteValidator.ValidateAsync(deleteCommand);

        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = "Validation failed",
                errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
            });
        }

        await Mediator.Send(deleteCommand);

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Deleted successfully.",
            errors = ""
        });
    }
}
