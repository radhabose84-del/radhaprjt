using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.RepackingMaster.Commands.CreateRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Commands.DeleteRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Commands.UpdateRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Queries.GetAllRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Queries.GetRepackingMasterAutoComplete;
using ProductionManagement.Application.RepackingMaster.Queries.GetRepackingMasterById;
using ProductionManagement.Application.RepackingMaster.Queries.GetStockItems;

namespace ProductionManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class RepackingMasterController : ApiControllerBase
    {
        public RepackingMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllRepackingMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllRepackingMasterQuery
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
        public async Task<IActionResult> GetRepackingMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetRepackingMasterByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetRepackingMasterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetRepackingMasterAutoCompleteQuery(term ?? string.Empty));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("getstockitems")]
        public async Task<IActionResult> GetStockItemsAsync(
            [FromQuery] int productionYear,
            [FromQuery] int? packTypeId = null)
        {
            var result = await Mediator.Send(new GetStockItemsQuery { ProductionYear = productionYear, PackTypeId = packTypeId });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRepackingMaster([FromBody] CreateRepackingMasterCommand command)
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
        public async Task<IActionResult> UpdateRepackingMaster([FromBody] UpdateRepackingMasterCommand command)
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
        public async Task<IActionResult> DeleteRepackingMaster(int id)
        {
            var result = await Mediator.Send(new DeleteRepackingMasterCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Repacking deleted successfully." : "Repacking not found."
            });
        }
    }
}
