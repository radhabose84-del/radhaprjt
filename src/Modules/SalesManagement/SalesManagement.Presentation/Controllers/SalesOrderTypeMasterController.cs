using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.CreateSalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.DeleteSalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.UpdateSalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Queries.GetAllSalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Queries.GetSalesOrderTypeMasterAutoComplete;
using SalesManagement.Application.SalesOrderTypeMaster.Queries.GetSalesOrderTypeMasterById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class SalesOrderTypeMasterController : ApiControllerBase
    {
        public SalesOrderTypeMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllSalesOrderTypeMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllSalesOrderTypeMasterQuery
            {
                PageNumber = PageNumber,
                PageSize   = PageSize,
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
        public async Task<IActionResult> GetSalesOrderTypeMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetSalesOrderTypeMasterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetSalesOrderTypeMasterAutoCompleteAsync(
            [FromQuery] string? term = null)
        {
            var result = await Mediator.Send(
                new GetSalesOrderTypeMasterAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalesOrderTypeMaster(
            [FromBody] CreateSalesOrderTypeMasterCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message   = result.Message,
                data      = result.Data
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSalesOrderTypeMaster(
            [FromBody] UpdateSalesOrderTypeMasterCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message   = result.Message,
                data      = result.Data
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSalesOrderTypeMaster(int id)
        {
            var result = await Mediator.Send(new DeleteSalesOrderTypeMasterCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message   = result
                    ? "Sales Order Type deleted successfully."
                    : "Failed to delete Sales Order Type."
            });
        }
    }
}
