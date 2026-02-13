using MaintenanceManagement.Application.Dashboard.DashboardQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.Presentation.Controllers.Dashboard
{
    [ApiController]
    [Route("api/maintenance/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("workOrder-summary")]
        public async Task<IActionResult> GetWorkOrderSummary([FromQuery] DashboardQuery request)
        {
            request.Type = "workOrderSummary";
            var data = await _mediator.Send(request);
            return Ok(data);
        }

        [HttpGet("item-consumption")]
        public async Task<IActionResult> GetItemConsumption([FromQuery] DashboardQuery request)
        {
            request.Type = "itemConsumption";
            var data = await _mediator.Send(request);
            return Ok(data);
        }
        [HttpGet("maintenance-hoursDept")]
        public async Task<IActionResult> GetMaintenanceHoursDept([FromQuery] DashboardQuery request)
        {
            request.Type = "maintenanceHrs-dept";

            var data = await _mediator.Send(request);
            return Ok(data);
        }
        [HttpGet("maintenance-hoursMachine")]
        public async Task<IActionResult> GetMaintenanceHours([FromQuery] DashboardQuery request)
        {
            request.Type = request.MachineGroupId != null ? "maintenanceHrs-machine" :
                        "maintenanceHrs-machineGroup";

            var data = await _mediator.Send(request);
            return Ok(data);
        }
        [HttpGet("itemConsumption-dept")]
        public async Task<IActionResult> GetItemConsumptionDept([FromQuery] DashboardQuery request)
        {
            request.Type = "itemConsumption-dept";

            var data = await _mediator.Send(request);
            return Ok(data);
        }
        [HttpGet("itemConsumption-machineGroup")]
        public async Task<IActionResult> GetItemConsumptionMachineGroup([FromQuery] DashboardQuery request)
        {
            request.Type = "itemConsumption-machineGroup";

            var data = await _mediator.Send(request);
            return Ok(data);
        }
        [HttpGet("card-dashboard")]
        public async Task<IActionResult> GetCardDashboard([FromQuery] CardViewQuery request)
        {
            var data = await _mediator.Send(request);
            return Ok(data);
        }
    }
}