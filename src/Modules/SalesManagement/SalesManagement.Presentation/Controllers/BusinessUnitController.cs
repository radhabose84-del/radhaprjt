using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.BusinessUnit.Commands.CreateBusinessUnit;
using SalesManagement.Application.BusinessUnit.Commands.DeleteBusinessUnit;
using SalesManagement.Application.BusinessUnit.Commands.UpdateBusinessUnit;
using SalesManagement.Application.BusinessUnit.Queries.GetAllBusinessUnit;
using SalesManagement.Application.BusinessUnit.Queries.GetBusinessUnitAutoComplete;
using SalesManagement.Application.BusinessUnit.Queries.GetBusinessUnitById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class BusinessUnitController : ApiControllerBase
    {
        public BusinessUnitController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllBusinessUnitAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllBusinessUnitQuery
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
        public async Task<IActionResult> GetBusinessUnitByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetBusinessUnitByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetBusinessUnitAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetBusinessUnitAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateBusinessUnit([FromBody] CreateBusinessUnitCommand command)
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
        public async Task<IActionResult> UpdateBusinessUnit([FromBody] UpdateBusinessUnitCommand command)
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
        public async Task<IActionResult> DeleteBusinessUnit(int id)
        {
            var result = await Mediator.Send(new DeleteBusinessUnitCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Business Unit deleted successfully." : "Failed to delete Business Unit."
            });
        }
    }
}
