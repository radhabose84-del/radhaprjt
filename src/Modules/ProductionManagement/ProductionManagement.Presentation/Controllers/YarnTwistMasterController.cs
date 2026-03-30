using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.YarnTwistMaster.Commands.CreateYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Commands.DeleteYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Commands.UpdateYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Queries.GetAllYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Queries.GetYarnTwistMasterAutoComplete;
using ProductionManagement.Application.YarnTwistMaster.Queries.GetYarnTwistMasterById;

namespace ProductionManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class YarnTwistMasterController : ApiControllerBase
    {
        public YarnTwistMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllYarnTwistMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllYarnTwistMasterQuery
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
        public async Task<IActionResult> GetYarnTwistMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetYarnTwistMasterByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetYarnTwistMasterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetYarnTwistMasterAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateYarnTwistMaster([FromBody] CreateYarnTwistMasterCommand command)
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
        public async Task<IActionResult> UpdateYarnTwistMaster([FromBody] UpdateYarnTwistMasterCommand command)
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
        public async Task<IActionResult> DeleteYarnTwistMaster(int id)
        {
            var result = await Mediator.Send(new DeleteYarnTwistMasterCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Yarn Twist Master deleted successfully." : "Yarn Twist Master not found."
            });
        }
    }
}
