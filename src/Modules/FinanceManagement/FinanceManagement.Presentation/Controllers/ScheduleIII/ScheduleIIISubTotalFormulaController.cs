using FinanceManagement.Application.ScheduleIII.Commands.SaveSubTotalFormula;
using FinanceManagement.Application.ScheduleIII.Queries.GetSubTotalFormulaOperands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // Schedule III sub-total formula operands — the signed lines of a sub-total's "Edit formula" dialog.
    // An operand is a P&L line item or another sub-total (off / + / −).
    [Route("api/finance/[controller]")]
    public class ScheduleIIISubTotalFormulaController : ApiControllerBase
    {
        public ScheduleIIISubTotalFormulaController(IMediator mediator) : base(mediator) { }

        // Operand picker for the "Edit formula" dialog: active P&L line items + other sub-totals.
        // Pass ?subTotalId= when editing an existing sub-total to get each operand's current +/− selection.
        [HttpGet("formula-operands")]
        public async Task<IActionResult> GetFormulaOperands([FromQuery] int? subTotalId = null)
        {
            var result = await Mediator.Send(new GetSubTotalFormulaOperandsQuery { SubTotalId = subTotalId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result.Data });
        }

        // Save the operand set of one sub-total (first time). Replaces any existing operands.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveSubTotalFormulaCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        // Replace the operand set of one sub-total. Old rows are physically deleted (logged to ActivityLog).
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SaveSubTotalFormulaCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }
    }
}
