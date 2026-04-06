using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.RepackingMaster.Queries.GetStockItems;
using ProductionManagement.Application.YarnConversionHeader.Commands.CreateYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Commands.DeleteYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Commands.UpdateYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Queries.GetAllYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Queries.GetYarnConversionHeaderAutoComplete;
using ProductionManagement.Application.YarnConversionHeader.Queries.GetYarnConversionHeaderById;

namespace ProductionManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class YarnConversionHeaderController : ApiControllerBase
    {
        public YarnConversionHeaderController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllYarnConversionHeaderAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllYarnConversionHeaderQuery
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
        public async Task<IActionResult> GetYarnConversionHeaderByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetYarnConversionHeaderByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetYarnConversionHeaderAutoCompleteAsync(
            [FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetYarnConversionHeaderAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("getstockitems")]
        public async Task<IActionResult> GetStockItemsAsync([FromQuery] int productionYear)
        {
            var result = await Mediator.Send(new GetStockItemsQuery { ProductionYear = productionYear });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateYarnConversionHeader(
            [FromBody] CreateYarnConversionHeaderCommand command)
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
        public async Task<IActionResult> UpdateYarnConversionHeader(
            [FromBody] UpdateYarnConversionHeaderCommand command)
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
        public async Task<IActionResult> DeleteYarnConversionHeader(int id)
        {
            var result = await Mediator.Send(new DeleteYarnConversionHeaderCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }
    }
}
