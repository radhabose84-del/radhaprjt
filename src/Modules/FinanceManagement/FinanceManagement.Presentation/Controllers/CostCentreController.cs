using FinanceManagement.Application.CostCentre.Commands.CreateCostCentre;
using FinanceManagement.Application.CostCentre.Commands.DeleteCostCentre;
using FinanceManagement.Application.CostCentre.Commands.UpdateCostCentre;
using FinanceManagement.Application.CostCentre.Queries.GetAllCostCentre;
using FinanceManagement.Application.CostCentre.Queries.GetCostCentreAutoComplete;
using FinanceManagement.Application.CostCentre.Queries.GetCostCentreById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class CostCentreController : ApiControllerBase
    {
        public CostCentreController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllCostCentreAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllCostCentreQuery
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
        public async Task<IActionResult> GetCostCentreByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetCostCentreByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        // Autocomplete + Parent-Cost-Centre picker. Pass `level` to restrict to a level
        // (e.g. an L2 create passes the L1 level id to list only Plant cost centres).
        [HttpGet("by-name")]
        public async Task<IActionResult> GetCostCentreAutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] int? level = null)
        {
            var result = await Mediator.Send(new GetCostCentreAutoCompleteQuery(term ?? string.Empty, level));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCostCentre([FromBody] CreateCostCentreCommand command)
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
        public async Task<IActionResult> UpdateCostCentre([FromBody] UpdateCostCentreCommand command)
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
        public async Task<IActionResult> DeleteCostCentre(int id)
        {
            var result = await Mediator.Send(new DeleteCostCentreCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Cost Centre deleted successfully." : "Failed to delete Cost Centre."
            });
        }
    }
}
