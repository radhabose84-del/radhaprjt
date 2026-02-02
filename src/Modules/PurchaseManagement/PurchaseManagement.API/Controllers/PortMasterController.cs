using PurchaseManagement.Application.Port.Commands;
using PurchaseManagement.Application.Port.Queries.GetAllPorts;
using PurchaseManagement.Application.Port.Queries.GetById;
using PurchaseManagement.Application.Port.Queries.GetPortAutocomplete;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.API.Controllers
{
    [Route("api/[controller]")]
    public class PortMasterController : ApiControllerBase
    {
        private readonly IValidator<CreatePortMasterCommand> _createValidator;
        private readonly IValidator<UpdatePortMasterCommand> _updateValidator;
        private readonly IValidator<DeletePortMasterCommand> _deleteValidator;

        public PortMasterController(
            ISender mediator,
            IValidator<CreatePortMasterCommand> createValidator,
            IValidator<UpdatePortMasterCommand> updateValidator,
            IValidator<DeletePortMasterCommand> deleteValidator
        ) : base(mediator)
        {
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _deleteValidator = deleteValidator;
        }

        // 🔹 GET ALL (Paged)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? CountryId = null,
            [FromQuery] byte? Type = null,
            [FromQuery] int? PortTypeId = null)
        {
            var result = await Mediator.Send(
                new GetAllPortsQuery(PageNumber, PageSize, SearchTerm, CountryId,  PortTypeId)
            );

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Items,
                TotalCount = result.Total,
                PageNumber,
                PageSize
            });
        }

        // 🔹 GET BY ID
        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var data = await Mediator.Send(new GetPortByIdQuery(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data,
                message = data
            });
        }

        // 🔹 Auto Complete
        [HttpGet("by-name")]
        public async Task<IActionResult> AutoComplete([FromQuery] string? name)
        {
            var data = await Mediator.Send(new GetPortAutocompleteQuery(name ?? string.Empty));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data });
        }

        // 🔹 CREATE
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreatePortMasterCommand command)
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

        // 🔹 UPDATE
        [HttpPut]
        public async Task<IActionResult> Update(UpdatePortMasterCommand command)
        {
            var validationResult = await _updateValidator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Updated Successfully",
                Errors = ""
            });
        }

        // 🔹 DELETE (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await Mediator.Send(new DeletePortMasterCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Deleted successfully.",
                errors = ""
            });
        }
    }
}
