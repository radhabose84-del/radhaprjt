using FinanceManagement.Application.ProfitCentre.Commands.CreateProfitCentre;
using FinanceManagement.Application.ProfitCentre.Commands.DeleteProfitCentre;
using FinanceManagement.Application.ProfitCentre.Commands.UpdateProfitCentre;
using FinanceManagement.Application.ProfitCentre.Queries.GetAllProfitCentre;
using FinanceManagement.Application.ProfitCentre.Queries.GetProfitCentreAutoComplete;
using FinanceManagement.Application.ProfitCentre.Queries.GetProfitCentreById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class ProfitCentreController : ApiControllerBase
    {
        public ProfitCentreController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllProfitCentreAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllProfitCentreQuery
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
        public async Task<IActionResult> GetProfitCentreByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetProfitCentreByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        // Autocomplete + Parent-Segment picker. Pass `level` to restrict to a level
        // (e.g. an L2 create passes the L1 level id to list only Segment profit centres).
        [HttpGet("by-name")]
        public async Task<IActionResult> GetProfitCentreAutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] int? level = null)
        {
            var result = await Mediator.Send(new GetProfitCentreAutoCompleteQuery(term ?? string.Empty, level));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfitCentre([FromBody] CreateProfitCentreCommand command)
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
        public async Task<IActionResult> UpdateProfitCentre([FromBody] UpdateProfitCentreCommand command)
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
        public async Task<IActionResult> DeleteProfitCentre(int id)
        {
            var result = await Mediator.Send(new DeleteProfitCentreCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Profit Centre deleted successfully." : "Failed to delete Profit Centre."
            });
        }
    }
}
