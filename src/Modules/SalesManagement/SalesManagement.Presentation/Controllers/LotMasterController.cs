using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.LotMaster.Commands.CreateLotMaster;
using SalesManagement.Application.LotMaster.Commands.DeleteLotMaster;
using SalesManagement.Application.LotMaster.Commands.UpdateLotMaster;
using SalesManagement.Application.LotMaster.Queries.GetAllLotMaster;
using SalesManagement.Application.LotMaster.Queries.GetLotMasterAutoComplete;
using SalesManagement.Application.LotMaster.Queries.GetLotMasterById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class LotMasterController : ApiControllerBase
    {
        public LotMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllLotMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllLotMasterQuery
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
        public async Task<IActionResult> GetLotMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetLotMasterByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetLotMasterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetLotMasterAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateLotMaster([FromBody] CreateLotMasterCommand command)
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
        public async Task<IActionResult> UpdateLotMaster([FromBody] UpdateLotMasterCommand command)
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
        public async Task<IActionResult> DeleteLotMaster(int id)
        {
            var result = await Mediator.Send(new DeleteLotMasterCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Lot Master deleted successfully." : "Lot Master not found."
            });
        }
    }
}
