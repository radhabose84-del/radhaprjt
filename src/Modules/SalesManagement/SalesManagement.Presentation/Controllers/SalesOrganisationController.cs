#nullable disable
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesOrganisation.Commands.CreateSalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.UpdateSalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.DeleteSalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Queries.GetAllSalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Queries.GetSalesOrganisationById;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Queries.GetSalesOrganisationAutoComplete;

namespace SalesManagement.Presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SalesOrganisationController : ApiControllerBase
    {
        public SalesOrganisationController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSalesOrganisationAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllSalesOrganisationQuery
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
        public async Task<IActionResult> GetSalesOrganisationByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetSalesOrganisationByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                message = result.Message,
                isSuccess = result.IsSuccess
            });
        }

        [HttpGet("autocomplete")]
        public async Task<IActionResult> GetSalesOrganisationAutoCompleteAsync(
            [FromQuery] string term = null)
        {
            var result = await Mediator.Send(new GetSalesOrganisationAutoCompleteQuery(term ?? string.Empty));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSalesOrganisation([FromBody] CreateSalesOrganisationCommand command)
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
        public async Task<IActionResult> UpdateSalesOrganisation([FromBody] UpdateSalesOrganisationCommand command)
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
        public async Task<IActionResult> DeleteSalesOrganisation(int id)
        {
            if (id <= 0)
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid Id provided." });

            var result = await Mediator.Send(new DeleteSalesOrganisationCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Sales Organisation deleted successfully." : "Failed to delete Sales Organisation.",
                data = result
            });
        }
    }
}
