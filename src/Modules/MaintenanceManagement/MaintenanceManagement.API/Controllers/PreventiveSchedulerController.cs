using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.ActiveInActivePreventive;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.CreatePreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.DeletePreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.MachineWiseFrequencyUpdate;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.MapMachine;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.RescheduleBulkImport;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.ReschedulePreventive;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.ScheduleWorkOrder;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.UpdatePreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetDetailSchedulerByDate;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveSchedulerById;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetSchedulerByDate;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetUnMappedMachine;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PreventiveSchedulerController : ApiControllerBase
    {
       
        public PreventiveSchedulerController(ISender mediator)
        : base(mediator)
        {
           
        }
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var response = await Mediator.Send(
             new GetPreventiveSchedulerQuery
             {
                 PageNumber = PageNumber,
                 PageSize = PageSize,
                 SearchTerm = SearchTerm
             });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = response.Data,
                TotalCount = response.TotalCount,
                PageNumber = response.PageNumber,
                PageSize = response.PageSize
            });
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreatePreventiveSchedulerCommand command)
        {

            var response = await Mediator.Send(command);
          
                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = "Preventive scheduler created successfully",
                    errors = "",
                    data = response
                });
           

        }
        [Route("[action]/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetByIdAsync(int id)
        {

            var preventiveScheduler = await Mediator.Send(new GetPreventiveSchedulerByIdQuery { Id = id });

            if (preventiveScheduler == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = $"PreventiveScheduler ID {id} not found.",
                    errors = ""
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = preventiveScheduler.Data
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdatePreventiveSchedulerCommand command)
        {
            
            var response = await Mediator.Send(command);
            if (response.IsSuccess)
            {
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    message = response.Message,
                    errors = ""
                });
            }



            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = response.Message,
                errors = ""
            });
        }


        [HttpDelete("{id}")]

        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeletePreventiveSchedulerCommand { Id = id };
          
             await Mediator.Send(command);

           
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    message = "Preventive Scheduler deleted successfully.",
                    errors = ""
                });

        }
        [HttpPut("reschedule")]
        public async Task<IActionResult> Reschedule(ReshedulePreventiveCommand command)
        {
          

            var response = await Mediator.Send(command);
            if (response.IsSuccess)
            {
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    message = response.Message,
                    errors = ""
                });
            }



            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = response.Message,
                errors = ""
            });
        }
        [HttpPut("UpdateActiveStatus")]
        public async Task<IActionResult> UpdateActiveInActive(ActiveInActivePreventiveCommand command)
        {
           

            var response = await Mediator.Send(command);
           
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    message = response,
                    errors = ""
                });
          
        }
        [HttpGet("SchedulerAbstractByDate")]
        public async Task<IActionResult> GetScheduler([FromQuery] int DepartmentId)
        {
            var response = await Mediator.Send(
             new GetSchedulerByDateQuery
             {
                 DepartmentId = DepartmentId
             });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = response.Data
            });
        }
        [HttpPost("SchedulerDetailByDate")]
        public async Task<IActionResult> GetSchedulerDetail(GetDetailSchedulerByDateQuery command)
        {
            var response = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = response.Data
            });
        }
        [HttpPost("HangfireSchedule")]
        [AllowAnonymous]
        public async Task<IActionResult> HangfireSchedule(ScheduleWorkOrderCommand command)
        {
            var response = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = response.Data
            });
        }
        [HttpPost("bulk-upload-schedule")]
        public async Task<IActionResult> UploadPreventiveSchedule(RescheduleBulkImportCommand command)
        {
           

            var result = await Mediator.Send(command);
            return Ok(result);
        }
        [HttpGet("MachineDetailBySchedule")]
        public async Task<IActionResult> GetMachineDetail([FromQuery] int Id)
        {
            var response = await Mediator.Send(new GetMachineDetailByIdQuery
            {
                Id = Id
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = response.Data
            });
        }
        [HttpGet("UnMappedMachines")]
        public async Task<IActionResult> GetUnMappedMachines([FromQuery] int Id)
        {
            var response = await Mediator.Send(new GetUnMappedMachineQuery
            {
                Id = Id
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = response.Data
            });
        }
        [HttpPost("MapMachines")]
        public async Task<IActionResult> MapMachines(MapMachineCommand command)
        {
           
            var response = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = response.Message
            });
        }
          [HttpPost("MachineFrequencyUpdate")]
        public async Task<IActionResult> MachineFrequencyUpdate(MachineWiseFrequencyUpdateCommand command)
        {
            
            var response = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = response.Message
            });
        }
    }
}