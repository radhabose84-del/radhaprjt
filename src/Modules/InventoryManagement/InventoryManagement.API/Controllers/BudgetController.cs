using InventoryManagement.Application.Budget.Commands.CreateBudget;
using InventoryManagement.Application.Budget.Commands.UpdateBudget;
using InventoryManagement.Application.Budget.Queries.GetAllBudgets;
using InventoryManagement.Application.Budget.Queries.GetBudgetById;
using InventoryManagement.Application.Budget.Queries.GetBudgetLogs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BudgetController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBudgetById(int id)
        {
            var response = await _mediator.Send(new GetBudgetByIdQuery { BudgetId = id });
            return Ok(response);
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetCommand command)
        {
            if (command == null)
                return BadRequest("Invalid budget data.");

            var result = await _mediator.Send(command);
            return Ok(new
            {
                Success = true,
                Message = "Budget created successfully.",
                BudgetId = result
            });
        }
        [HttpPut("update")]
        public async Task<IActionResult> UpdateBudget([FromBody] UpdateBudgetCommand command)
        {
            if (command == null || command.BudgetId <= 0)
                return BadRequest("Invalid budget update data.");

            var result = await _mediator.Send(command);
            return Ok(new
            {
                Success = result,
                Message = result ? "Budget updated successfully." : "Budget update failed."
            });
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllBudgets([FromQuery] int? fiscalYear)
        {
            var result = await _mediator.Send(new GetAllBudgetsQuery
            {
                FiscalYear = fiscalYear
            });

            return Ok(new
            {
                Success = true,
                Data = result
            });
        }
        [HttpGet("logs")]
        public async Task<IActionResult> GetBudgetLogs([FromQuery] int? budgetId, [FromQuery] int? budgetDetailId)
        {
            var logs = await _mediator.Send(new GetBudgetLogsQuery
            {
                BudgetId = budgetId,
                BudgetDetailId = budgetDetailId
            });

            return Ok(new
            {
                Success = true,
                Data = logs
            });
        }
    }
}
