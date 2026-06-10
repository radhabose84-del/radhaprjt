using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.CreateVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.UpdateVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.DeleteVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Queries.GetAllVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Queries.GetVehicleMovementRecordById;
using GateEntryManagement.Application.VehicleMovementRecord.Queries.GetVehicleMovementRecordAutoComplete;
using GateEntryManagement.Application.VehicleMovementRecord.Queries.GetPendingVehicle;

namespace GateEntryManagement.Presentation.Controllers
{
    [Route("api/gateentry/[controller]")]
    public class VehicleMovementRecordController : ApiControllerBase
    {
        public VehicleMovementRecordController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllVehicleMovementRecordAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllVehicleMovementRecordQuery
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
        public async Task<IActionResult> GetVehicleMovementRecordByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetVehicleMovementRecordByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetVehicleMovementRecordAutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] int? purposeOfVisitId = null)
        {
            var result = await Mediator.Send(new GetVehicleMovementRecordAutoCompleteQuery(term ?? string.Empty, purposeOfVisitId));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingVehiclesAsync(
            [FromQuery] string? VehicleMovementId = null,
            [FromQuery] string? VehicleNumber = null)
        {
            var result = await Mediator.Send(new GetPendingVehicleQuery
            {
                VehicleMovementId = VehicleMovementId,
                VehicleNumber = VehicleNumber
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicleMovementRecord([FromBody] CreateVehicleMovementRecordCommand command)
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
        public async Task<IActionResult> UpdateVehicleMovementRecord([FromBody] UpdateVehicleMovementRecordCommand command)
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
        public async Task<IActionResult> DeleteVehicleMovementRecord(int id)
        {
            var result = await Mediator.Send(new DeleteVehicleMovementRecordCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Vehicle Movement Record deleted successfully." : "Failed to delete Vehicle Movement Record."
            });
        }
    }
}
