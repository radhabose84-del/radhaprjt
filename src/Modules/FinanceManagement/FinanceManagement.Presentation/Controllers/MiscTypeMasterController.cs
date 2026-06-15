using FinanceManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using FinanceManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster;
using FinanceManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using FinanceManagement.Application.MiscTypeMaster.Queries.GetAllMiscTypeMaster;
using FinanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using FinanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class MiscTypeMasterController : ApiControllerBase
    {
        public MiscTypeMasterController(IMediator mediator) : base(mediator) { }

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
