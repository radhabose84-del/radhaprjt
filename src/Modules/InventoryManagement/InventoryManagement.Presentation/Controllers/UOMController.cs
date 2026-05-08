#nullable disable
using InventoryManagement.Application.UOM.Command.CreateUOM;
using InventoryManagement.Application.UOM.Command.DeleteUOM;
using InventoryManagement.Application.UOM.Command.UpdateUOM;
using InventoryManagement.Application.UOM.Queries.GetUOMAutoComplete;
using InventoryManagement.Application.UOM.Queries.GetUOMById;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using InventoryManagement.Application.UOM.Queries.GetUOMTypeAutoComplete;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace InventoryManagement.Presentation.Controllers
{
    [Route("api/inventory/[controller]")]
    public class UOMController : ApiControllerBase
    {
        private readonly IValidator<CreateUOMCommand> _createUOMCommandValidator;
        private readonly IValidator<UpdateUOMCommand> _updateUOMCommandValidator;

        public UOMController(ISender mediator, IValidator<CreateUOMCommand> createUOMCommandValidator, IValidator<UpdateUOMCommand> updateUOMCommandValidator)
        : base(mediator)
        {
            _createUOMCommandValidator = createUOMCommandValidator;
            _updateUOMCommandValidator = updateUOMCommandValidator;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUOMAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string SearchTerm = null)
        {
            var uom = await Mediator.Send(
                new GetUOMQuery
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    SearchTerm = SearchTerm
                });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = uom.Data.ToList(),
                TotalCount = uom.TotalCount,
                PageNumber = uom.PageNumber,
                PageSize = uom.PageSize
            });
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateUOMCommand createuomcommand)
        {

            var validationResult = await _createUOMCommandValidator.ValidateAsync(createuomcommand);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest, message = "Validation failed",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
                });
            }
            var result = await Mediator.Send(createuomcommand);
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = result.Message,
                    data = result.Data
                });
            }
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = result.Message
            });

        }
        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid UOM ID"
                });
            }
            var result = await Mediator.Send(new GetUOMByIdQuery() { Id = id });

            if (!result.IsSuccess)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = result.Message
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateUOMCommand updateUomcommand)
        {
            var validationResult = await _updateUOMCommandValidator.ValidateAsync(updateUomcommand);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }


            var uomExists = await Mediator.Send(new GetUOMByIdQuery { Id = updateUomcommand.Id });

            if (uomExists == null)
            {
                return NotFound(new { StatusCode = StatusCodes.Status404NotFound, message = $"UOM ID {updateUomcommand.Id} not found.", errors = "" });
            }

            var result = await Mediator.Send(updateUomcommand);
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = result.Message,
                    data = result.Data
                });
            }
            else
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = result.Message
                });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid UOM ID"
                });
            }
            var deleteduom = await Mediator.Send(new DeleteUOMCommand { Id = id });

            if (!deleteduom.IsSuccess)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = deleteduom.Message
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,                
                message = deleteduom.Message
            });

        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetUOM([FromQuery] string name, [FromQuery] string uomTypeCode = null)
        {
            var result = await Mediator.Send(new GetUOMAutoCompleteQuery { SearchPattern = name, UOMTypeCode = uomTypeCode });
            if (!result.IsSuccess)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = result.Message
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result.Message,
                data = result.Data
            });
        }
        [HttpGet("by-Type")]
        public async Task<IActionResult> GetUOMType([FromQuery] string name)
        {
            var result = await Mediator.Send(new GetUOMTypeAutoCompleteQuery { SearchPattern = name });
            if (!result.IsSuccess)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = result.Message
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result.Message,
                data = result.Data
            });
        }
    
    }
}