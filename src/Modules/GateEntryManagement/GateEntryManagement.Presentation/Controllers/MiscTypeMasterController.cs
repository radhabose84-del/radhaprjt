using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GateEntryManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using GateEntryManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using GateEntryManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster;
using GateEntryManagement.Application.MiscTypeMaster.Queries.GetAllMiscTypeMaster;
using GateEntryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using GateEntryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;

namespace GateEntryManagement.Presentation.Controllers
{
    [Route("api/gateentry/[controller]")]
    public class MiscTypeMasterController : ApiControllerBase
    {
        public MiscTypeMasterController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllMiscTypeMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllMiscTypeMasterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMiscTypeMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetMiscTypeMasterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetMiscTypeMasterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetMiscTypeMasterAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateMiscTypeMaster([FromBody] CreateMiscTypeMasterCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMiscTypeMaster([FromBody] UpdateMiscTypeMasterCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMiscTypeMaster(int id)
        {
            var result = await Mediator.Send(new DeleteMiscTypeMasterCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Misc Type Master deleted successfully." : "Failed to delete Misc Type Master."
            });
        }
    }
}
