using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Commands.CreateService;
using PurchaseManagement.Application.ServiceMaster.Commands.DeleteService;
using PurchaseManagement.Application.ServiceMaster.Commands.UpdateService;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.Application.ServiceMaster.Queries.GetServiceAutocomplete;
using PurchaseManagement.Application.ServiceMaster.Queries.GetServiceById;
using FluentValidation;
using MassTransit.Futures.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.API.Controllers
{
    [Route("api/[controller]")]
    public class ServiceMasterController : ApiControllerBase
    {
        private readonly ILogger<ServiceMasterController> _logger;

        private readonly IServiceQueryRepository _serviceMasterQueryRepository;
        private readonly IValidator<CreateServiceCommand> _createServiceValidator;
        private readonly IValidator<UpdateServiceCommand> _updateServiceValidator;
        private readonly IValidator<DeleteServiceCommand> _deleteServiceValidator;
        private readonly IMediator _mediator;

        public ServiceMasterController(ISender mediator, ILogger<ServiceMasterController> logger, IServiceQueryRepository serviceMasterQueryRepository, IValidator<CreateServiceCommand> createServiceValidator, IValidator<UpdateServiceCommand> updateServiceValidator
        , IValidator<DeleteServiceCommand> deleteServiceValidator) : base(mediator)
        {
            _logger = logger;
            _serviceMasterQueryRepository = serviceMasterQueryRepository;
            _createServiceValidator = createServiceValidator;
            _updateServiceValidator = updateServiceValidator;
            _deleteServiceValidator = deleteServiceValidator;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllServiceMasterAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm)
        {
            var ServiceMaster = await Mediator.Send(new GetAllServicesMasterQuery { PageNumber = PageNumber, PageSize = PageSize, SearchTerm = SearchTerm });


            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = ServiceMaster.Data,
                TotalCount = ServiceMaster.TotalCount,
                PageNumber = ServiceMaster.PageNumber,
                PageSize = ServiceMaster.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceMasterByIdAsync(int id)
        {
            // return Ok(await Mediator.Send(new GetServiceMasterByIdQuery { Id = id }));

            var serviceMaster = await Mediator.Send(new GetServiceByIdQuery() { Id = id });

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = serviceMaster, message = serviceMaster });
        }


        [HttpPost]
        public async Task<IActionResult> CreateServiceMasterAsync([FromBody] CreateServiceCommand command)
        {
            // 1) Manual validation (same style as your Misc sample)
            var validationResult = await _createServiceValidator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Validation failed",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
                });
            }

            var dto = await Mediator.Send(command);

            return StatusCode(StatusCodes.Status201Created, new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created Successfully",
                errors = new string[] { },
                data = dto
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateServiceMasterAsync([FromBody] UpdateServiceCommand command)
        {
            var validationResult = await _updateServiceValidator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Validation failed",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
                });
            }

            var dto = await Mediator.Send(command);

            return StatusCode(StatusCodes.Status200OK, new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Updated Successfully"

            });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            
            await Mediator.Send(new DeleteServiceCommand { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Deleted successfully.",
                errors = ""
            });


        }

        [HttpGet("by-name")]

        public async Task<IActionResult>  GetServiceMaster([FromQuery] string? name)
        {
            var services = await Mediator.Send(new GetServiceAutocompleteQuery { SearchPattern = name });
                                
            return Ok( new
            {
                StatusCode = StatusCodes.Status200OK,
                data = services
            });
        }


         
        
       
    }
}