using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.Maintenance.WorkOrder.Command.UpdateWorkOrderRequestDate;
using MaintenanceManagement.Application.Reports.WorkOrderItemConsuption;
using MaintenanceManagement.Application.WorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder.CreateSchedule;
using MaintenanceManagement.Application.WorkOrder.Command.DeleteFileWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.DeleteFileWorkOrder.Item;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder.UpdateSchedule;
using MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder.Item;
using MaintenanceManagement.Application.WorkOrder.Queries.GetRequestType;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOderDropdown;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderById;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderRootCause;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderSource;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderStatus;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderStoreType;
using FluentValidation;
using MaintenanceManagement.API.Validation.WorkOrder;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkOrderController : ApiControllerBase
    {
    private readonly IWorkOrderCommandRepository _workOrderCommandRepository;

        public WorkOrderController(
            ISender mediator,
            IWorkOrderCommandRepository workOrderCommandRepository)
            : base(mediator)
        {
            _workOrderCommandRepository = workOrderCommandRepository;
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateWorkOrderCommand command)
        {
           
            var result = await Mediator.Send(command);
            if (result.IsSuccess)
            {

                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,

                    message = result.Message,
                    data = result.Data
                });

            }
            else

            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = result.Message
                });

            }
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateWorkOrderCommand command)
        {
          
            var result = await Mediator.Send(command);
            if (result.IsSuccess)
            {

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,

                    message = result.Message,
                    asset = result.Data
                });
            }
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = result.Message
            });
        }
        [HttpPost("schedule/Create")]
        public async Task<IActionResult> CreateScheduleAsync(CreateWOScheduleCommand command)
        {
        
            var result = await Mediator.Send(command);
            if (result.IsSuccess)
            {

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    message = result.Message,
                    //asset = result.Data
                });
            }
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = result.Message
            });
        }
        [HttpPut("schedule/Update")]
        public async Task<IActionResult> UpdateScheduleAsync(UpdateWOScheduleCommand command)
        {
           
            var result = await Mediator.Send(command);
            if (result.IsSuccess)
            {

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,

                    message = result.Message,
                    asset = result.Data
                });
            }
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = result.Message
            });
        }
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadLogo(UploadFileWorkOrderCommand uploadFileCommand)
        {
          
            var file = await Mediator.Send(uploadFileCommand);
            if (!file.IsSuccess)
            {

                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = file.Message,
                    errors = ""
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = file.Message,
                data = file.Data,
                errors = ""
            });
        }
        [HttpDelete("delete-image")]
        public async Task<IActionResult> DeleteLogo([FromBody] DeleteFileWorkOrderCommand deleteFileCommand)
        {
            if (deleteFileCommand == null || string.IsNullOrWhiteSpace(deleteFileCommand.Image))
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid request. 'ImagePath' cannot be null or empty.",
                    errors = ""
                });
            }

            var file = await Mediator.Send(deleteFileCommand);
            if (!file.IsSuccess)
            {

                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = file.Message,
                    errors = ""
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = file.Message,
                errors = ""
            });
        }
        [HttpPost("upload-image/Item")]
        public async Task<IActionResult> UploadItemLogo(UploadFileItemCommand uploadFileCommand)
        {
           
            var file = await Mediator.Send(uploadFileCommand);
            if (!file.IsSuccess)
            {

                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = file.Message,
                    errors = ""
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = file.Message,
                data = file.Data,
                errors = ""
            });
        }
        [HttpDelete("delete-image/Item")]
        public async Task<IActionResult> DeleteItemLogo([FromBody] DeleteFileItemCommand deleteFileCommand)
        {
            if (deleteFileCommand == null || string.IsNullOrWhiteSpace(deleteFileCommand.Image))
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid request. 'ImagePath' cannot be null or empty.",
                    errors = ""
                });
            }

            var file = await Mediator.Send(deleteFileCommand);
            if (!file.IsSuccess)
            {

                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = file.Message,
                    errors = ""
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = file.Message,
                errors = ""
            });
        }
        [HttpGet("Status")]
        public async Task<IActionResult> GetWorkOrderStatus()
        {
            var result = await Mediator.Send(new GetWorkOrderStatusQuery());
            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No WorkOrder Status found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "WorkOrder Status fetched successfully.",
                data = result.Data
            });
        }
        [HttpGet("RootCause")]
        public async Task<IActionResult> GetWorkOrderRootCause()
        {
            var result = await Mediator.Send(new GetWorkOrderRootCauseQuery());
            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No WorkOrder Status found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "WorkOrder Status fetched successfully.",
                data = result.Data
            });
        }
        [HttpGet("Source")]
        public async Task<IActionResult> GetWorkOrderSource()
        {
            var result = await Mediator.Send(new GetWorkOrderSourceQuery());
            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No WorkOrder Status found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "WorkOrder Status fetched successfully.",
                data = result.Data
            });
        }
        [HttpGet("StoreType")]
        public async Task<IActionResult> GetStoreType()
        {
            var result = await Mediator.Send(new GetWorkOrderStoreTypeQuery());
            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No StoreType found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "StoreType fetched successfully.",
                data = result.Data
            });
        }
        [HttpGet("RequestType")]
        public async Task<IActionResult> GetRequestType()
        {
            var result = await Mediator.Send(new GetRequestTypeQuery());
            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No Request Type found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "RequestType fetched successfully.",
                data = result.Data
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            if (id <= 0)
            {

                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,

                    message = "Invalid WorkOrder ID"
                });
            }
            var result = await Mediator.Send(new GetWorkOrderByIdQuery { Id = id });
            if (result is null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,

                    message = $"WorkOrder ID {id} not found",
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data

            });
        }
        [HttpGet]
        public async Task<IActionResult> GetByAllAsync([FromQuery] string? fromDate, [FromQuery] string? toDate, [FromQuery] int? requestTypeId
       , [FromQuery] int? departmentId, [FromQuery] int? machineId)
        {
            DateTimeOffset? parsedStartDate = null;
            DateTimeOffset? parsedEndDate = null;

            if (!string.IsNullOrWhiteSpace(fromDate))  // Allow null or empty values
            {
                if (!DateTimeOffset.TryParse(fromDate, out var parsedDate))
                {
                    return BadRequest(new { message = "Invalid fromDate format. Use yyyy-MM-dd." });
                }
                parsedStartDate = parsedDate;
            }

            if (!string.IsNullOrWhiteSpace(toDate))  // Allow null or empty values
            {
                if (!DateTimeOffset.TryParse(toDate, out var parsedDate))
                {
                    return BadRequest(new { message = "Invalid toDate format. Use yyyy-MM-dd." });
                }
                parsedEndDate = parsedDate;
            }
            if (requestTypeId <= 0)
            {
                return BadRequest(new { message = "Invalid Request Id" });
            }
            var workOrder = await Mediator.Send(
               new GetWorkOrderQuery
               {
                   fromDate = parsedStartDate,
                   toDate = parsedEndDate,
                   requestTypeId = requestTypeId,
                   departmentId = departmentId,
                   machineId = machineId
               });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = workOrder.Message,
                data = workOrder.Data.ToList()
            });
        }
        [HttpGet("GetWorkOrderDropdown")]
        public async Task<IActionResult> GetWorkOrderAsync()
        {
            var result = await Mediator.Send(new GetWorkOderDropdownQuery());
            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No Workorder found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Workorder fetched successfully.",
                data = result.Data
            });
        }
        [HttpPut("requestdate")]      
        public async Task<IActionResult> UpdateRequestDateAsync(            
            [FromBody] UpdateRequestDateDto dto,
            CancellationToken cancellationToken)
      {
            if (dto == null)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Request body is required."
                });
            }

            var cmd = new UpdateWorkOrderRequestDateCommand
            {
                WorkOrderId = dto.workOrderId,
                RequestDate = dto.RequestDate,
                IsSystemTime = dto.IsSystemTime
            };

            var success = await Mediator.Send(cmd, cancellationToken);

            if (!success)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = $"WorkOrder with Id {dto.workOrderId} not found."
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "WorkOrder request date updated successfully.",
                data = new
                {
                    workOrderId = dto.workOrderId,
                    requestDate = dto.RequestDate,
                    isSystemTime = dto.IsSystemTime
                }
            });
        }

        
    }
}