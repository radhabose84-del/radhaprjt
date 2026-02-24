#nullable disable
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesOffice.Commands.CreateSalesOffice;
using SalesManagement.Application.SalesOffice.Commands.UpdateSalesOffice;
using SalesManagement.Application.SalesOffice.Commands.DeleteSalesOffice;
using SalesManagement.Application.SalesOffice.Queries.GetAllSalesOffice;
using SalesManagement.Application.SalesOffice.Queries.GetSalesOfficeById;
using SalesManagement.Application.SalesOffice.Queries.GetSalesOfficeAutoComplete;

namespace SalesManagement.Presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SalesOfficeController : ApiControllerBase
    {
        public SalesOfficeController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSalesOfficeAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllSalesOfficeQuery
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
        public async Task<IActionResult> GetSalesOfficeByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetSalesOfficeByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                message = result.Message,
                isSuccess = result.IsSuccess
            });
        }

        [HttpGet("autocomplete")]
        public async Task<IActionResult> GetSalesOfficeAutoCompleteAsync(
            [FromQuery] string term = null)
        {
            var result = await Mediator.Send(new GetSalesOfficeAutoCompleteQuery(term ?? string.Empty));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSalesOffice([FromBody] CreateSalesOfficeCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateSalesOffice([FromBody] UpdateSalesOfficeCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (command.Id <= 0)
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid Id provided." });

            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteSalesOffice(int id)
        {
            if (id <= 0)
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid Id provided." });

            var result = await Mediator.Send(new DeleteSalesOfficeCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Sales Office deleted successfully." : "Failed to delete Sales Office.",
                data = result
            });
        }
    }
}
