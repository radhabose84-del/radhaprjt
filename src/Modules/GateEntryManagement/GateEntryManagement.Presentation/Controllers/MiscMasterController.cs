using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GateEntryManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using GateEntryManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using GateEntryManagement.Application.MiscMaster.Commands.DeleteMiscMaster;
using GateEntryManagement.Application.MiscMaster.Queries.GetAllMiscMaster;
using GateEntryManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using GateEntryManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;

namespace GateEntryManagement.Presentation.Controllers
{
    [Route("api/gateentry/[controller]")]
    public class MiscMasterController : ApiControllerBase
    {
        public MiscMasterController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllMiscMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? MiscTypeId = null)
        {
            var result = await Mediator.Send(new GetAllMiscMasterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                MiscTypeId = MiscTypeId
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
        public async Task<IActionResult> GetMiscMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetMiscMasterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetMiscMasterAutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] string? MiscTypeCode = null)
        {
            var result = await Mediator.Send(new GetMiscMasterAutoCompleteQuery(term ?? string.Empty, MiscTypeCode ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateMiscMaster([FromBody] CreateMiscMasterCommand command)
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
        public async Task<IActionResult> UpdateMiscMaster([FromBody] UpdateMiscMasterCommand command)
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
        public async Task<IActionResult> DeleteMiscMaster(int id)
        {
            var result = await Mediator.Send(new DeleteMiscMasterCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Misc Master deleted successfully." : "Failed to delete Misc Master."
            });
        }
    }
}
