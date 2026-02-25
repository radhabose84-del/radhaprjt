using BackgroundService.API.Controller.Notification;
using BackgroundService.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.API.Controllers
{        
   [Route("api/backgroundservice/[controller]")]
    public class MiscTypeMasterController : ApiControllerBase
    {

        //     private  readonly IValidator<CreateMiscTypeMasterCommand> _miscTypeMasterCommand;
        // private readonly IValidator<UpdateMiscTypeMasterCommand> _updateMiscTypeMasterCommand;

        // private readonly IValidator<DeleteMiscTypeMasterCommand> _deleteMiscTypeMasterCommand;
        public MiscTypeMasterController(ISender mediator  ) : base(mediator)

        {
            // _miscTypeMasterCommand = miscMasterCommand;
            // _updateMiscTypeMasterCommand = updateMiscMasterCommand;
            // _deleteMiscTypeMasterCommand = deleteMiscMasterCommand;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllMiscTypeMasterAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var misctypemaster = await Mediator.Send(
            new GetMiscTypeMasterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });
            // var activecompanies = companies.Data.ToList(); 

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = misctypemaster.Data,
                TotalCount = misctypemaster.TotalCount,
                PageNumber = misctypemaster.PageNumber,
                PageSize = misctypemaster.PageSize
            });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {

            var misctypemaster = await Mediator.Send(new GetMiscTypeMasterByIdQuery() { Id = id });

            if (misctypemaster.IsSuccess)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, data = misctypemaster.Data, message = misctypemaster.Message });
            }

            return NotFound(new { StatusCode = StatusCodes.Status404NotFound, message = $"MiscTypeMaster ID {id} not found.", errors = "" });

        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetMiscTypeMaster([FromQuery] string? name)
        {

            var misctypemaster = await Mediator.Send(new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = name });
            if (misctypemaster.IsSuccess)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, data = misctypemaster.Data });
            }
            return NotFound(new { StatusCode = misctypemaster.Message });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateMiscTypeMasterCommand command)
        {
            // var validationResult = await _miscTypeMasterCommand.ValidateAsync(command);

            // if (!validationResult.IsValid)
            // {
            //     return BadRequest(new
            //     {
            //         StatusCode = StatusCodes.Status400BadRequest,
            //         message = "Validation failed",
            //         errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
            //     });
            // }

            var response = await Mediator.Send(command);
            if (response.IsSuccess)
            {
                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = response.Message,
                    errors = "",
                    data = response.Data
                });
            }


            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = response.Message,
                errors = ""
            });

        }


        [HttpPut]
        public async Task<IActionResult> Update(UpdateMiscTypeMasterCommand command)
        {

            //   var validationResult = await _updateMiscTypeMasterCommand.ValidateAsync(command);

            // if (!validationResult.IsValid)
            // {
            //     return BadRequest(new
            //     {
            //         StatusCode = StatusCodes.Status400BadRequest,
            //         message = "Validation failed",
            //         errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
            //     });
            // }
            var misctypeExists = await Mediator.Send(new GetMiscTypeMasterByIdQuery { Id = command.Id });

            if (misctypeExists == null)
            {
                return NotFound(new { StatusCode = StatusCodes.Status404NotFound, message = $"MiscTypeMaster ID {command.Id} not found.", errors = "" });
            }

            var response = await Mediator.Send(command);
            if (response.IsSuccess)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, message = response.Message, errors = "" });
            }



            return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, message = response.Message, errors = "" });
        }

        [HttpDelete("{id}")]

        public async Task<IActionResult> Delete(int id)
        {
            
            var updatedDivision = await Mediator.Send(new DeleteMiscTypeMasterCommand { Id = id });

            if (updatedDivision.IsSuccess)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, message = updatedDivision.Message, errors = "" });

            }

            return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, message = updatedDivision.Message, errors = "" });

        }

    }
}