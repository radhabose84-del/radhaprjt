using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.FreightMaster.Commands.CreateFreightMaster;
using SalesManagement.Application.FreightMaster.Commands.DeleteFreightMaster;
using SalesManagement.Application.FreightMaster.Commands.UpdateFreightMaster;
using SalesManagement.Application.FreightMaster.Queries.GetAllFreightMaster;
using SalesManagement.Application.FreightMaster.Queries.GetFreightMasterAutoComplete;
using SalesManagement.Application.FreightMaster.Queries.GetFreightMasterById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class FreightMasterController : ApiControllerBase
    {
        public FreightMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllFreightMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllFreightMasterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                result.TotalCount,
                result.PageNumber,
                result.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreightMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetFreightMasterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetFreightMasterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetFreightMasterAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateFreightMaster([FromBody] CreateFreightMasterCommand command)
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
        public async Task<IActionResult> UpdateFreightMaster([FromBody] UpdateFreightMasterCommand command)
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
        public async Task<IActionResult> DeleteFreightMaster(int id)
        {
            var result = await Mediator.Send(new DeleteFreightMasterCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "FreightMaster deleted successfully." : "Failed to delete FreightMaster."
            });
        }
    }
}
