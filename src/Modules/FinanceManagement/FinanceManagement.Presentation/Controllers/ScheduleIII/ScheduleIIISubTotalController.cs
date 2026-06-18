using FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateSubTotal;
using FinanceManagement.Application.ScheduleIII.Queries.GetSubTotalFormulaOperands;
using FinanceManagement.Application.ScheduleIII.Queries.GetSubTotals;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // Schedule III sub-total nodes (Gross Profit / EBITDA / PBT / PAT). Structure = token company/division.
    [Route("api/finance/[controller]")]
    public class ScheduleIIISubTotalController : ApiControllerBase
    {
        public ScheduleIIISubTotalController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await Mediator.Send(new GetSubTotalsQuery());
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result.Data });
        }

        // Operand picker for the "Edit formula" dialog: active P&L line items + S3_SUBTOTAL_TYPE nodes.
        [HttpGet("formula-operands")]
        public async Task<IActionResult> GetFormulaOperands()
        {
            var result = await Mediator.Send(new GetSubTotalFormulaOperandsQuery());
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result.Data });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubTotalCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateSubTotalCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }
    }
}
