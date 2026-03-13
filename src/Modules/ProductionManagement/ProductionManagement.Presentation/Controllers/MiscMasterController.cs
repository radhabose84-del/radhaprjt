using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using ProductionManagement.Application.MiscMaster.Commands.DeleteMiscMaster;
using ProductionManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using ProductionManagement.Application.MiscMaster.Queries.GetAllMiscMaster;
using ProductionManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using ProductionManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using ProductionManagement.Presentation.Controllers;

namespace ProductionManagement.Presentation.Controllers
{
    [Route("api/production/[controller]")]
    public class MiscMasterController : ApiControllerBase
    {
        public MiscMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllMiscMasterAsync(
            [FromQuery] int PageNumber = 1,
            [FromQuery] int PageSize = 10,
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
            [FromQuery] string? miscTypeCode = null)
        {
            var result = await Mediator.Send(new GetMiscMasterAutoCompleteQuery(term, miscTypeCode));
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
                message = result ? "Misc Master deleted successfully." : "Misc Master not found."
            });
        }
    }
}
