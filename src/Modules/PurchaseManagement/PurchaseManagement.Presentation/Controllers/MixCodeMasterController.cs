using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.MixCodeMaster.Commands.CreateMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Commands.DeleteMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Commands.UpdateMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Queries.GetAllMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Queries.GetMixCodeMasterAutoComplete;
using PurchaseManagement.Application.MixCodeMaster.Queries.GetMixCodeMasterById;

namespace PurchaseManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class MixCodeMasterController : ApiControllerBase
    {
        public MixCodeMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllMixCodeMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllMixCodeMasterQuery
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
        public async Task<IActionResult> GetMixCodeMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetMixCodeMasterByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetMixCodeMasterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetMixCodeMasterAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateMixCodeMaster([FromBody] CreateMixCodeMasterCommand command)
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
        public async Task<IActionResult> UpdateMixCodeMaster([FromBody] UpdateMixCodeMasterCommand command)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMixCodeMaster(int id)
        {
            var result = await Mediator.Send(new DeleteMixCodeMasterCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Mix Code deleted successfully." : "Mix Code not found.",
                data = result
            });
        }
    }
}
