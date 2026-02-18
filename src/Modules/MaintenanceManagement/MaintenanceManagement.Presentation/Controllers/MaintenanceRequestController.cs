#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Microsoft.Extensions.Logging;
using MediatR;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestById;
using Contracts.Common;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestCommand;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExistingVendorDetails;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestStatusCommand;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceExternalRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateExternalRequestWorkOrder;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExternalRequestById;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestType;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceServiceType;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceServiceLocation;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceSparesType;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceDipatchMode;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.Reports.MaintenanceRequestReport;
using Microsoft.AspNetCore.Http;

namespace MaintenanceManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class MaintenanceRequestController : ApiControllerBase
    {
        private readonly ILogger<MaintenanceRequestController> _logger;

         private readonly IValidator<CreateMaintenanceRequestCommand> _createmaintenancecommandvalidator;

         private readonly IValidator<UpdateMaintenanceRequestCommand> _updateMaintenanceRequestCommandValidator;
         private readonly IMaintenanceCategoryQueryRepository _maintenanceCategoryQueryRepository;

         private readonly IMaintenanceRequestQueryRepository _maintenanceRequestQueryRepository;

            
        public MaintenanceRequestController(ISender mediator,ILogger<MaintenanceRequestController> logger,IValidator<CreateMaintenanceRequestCommand> createmaintenancecommandvalidator,IValidator<UpdateMaintenanceRequestCommand> updatemaintenancerequestcommandvalidator,IMaintenanceCategoryQueryRepository maintenanceCategoryQueryRepository ,IMaintenanceRequestQueryRepository maintenanceRequestQueryRepository)
        : base(mediator)
        {
            _logger = logger;
            _createmaintenancecommandvalidator = createmaintenancecommandvalidator;
            _updateMaintenanceRequestCommandValidator = updatemaintenancerequestcommandvalidator;
            _maintenanceCategoryQueryRepository = maintenanceCategoryQueryRepository;
            _maintenanceRequestQueryRepository = maintenanceRequestQueryRepository;
           
        }


        [HttpGet("InternalRequest")]
        public async Task<IActionResult> GetAllMaintenanceRequestAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string SearchTerm = null, [FromQuery] DateTimeOffset FromDate = default, [FromQuery] DateTimeOffset ToDate = default)
        {
            var maintenancerequest = await Mediator.Send(
            new GetMaintenanceRequestQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                FromDate = FromDate,
                ToDate = ToDate

            });


            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = maintenancerequest.Data,
                TotalCount = maintenancerequest.TotalCount,
                PageNumber = maintenancerequest.PageNumber,
                PageSize = maintenancerequest.PageSize
            });
          //  return Ok(maintenancerequest);
        }
         [HttpGet("ExternalRequest")] 
        public async Task<IActionResult> GetAllMaintenanceExternalRequestAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string SearchTerm = null , [FromQuery] DateTimeOffset FromDate = default, [FromQuery] DateTimeOffset ToDate = default)
        {
            var maintenanceexternalrequest = await Mediator.Send(
            new GetMaintenanceExternalRequestQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm,
                FromDate = FromDate,
                ToDate = ToDate
            });
           

            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = maintenanceexternalrequest.Data,
                TotalCount = maintenanceexternalrequest.TotalCount,
                PageNumber = maintenanceexternalrequest.PageNumber,
                PageSize = maintenanceexternalrequest.PageSize
            });
        }


          [HttpGet("Internal-requests/{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var maintenancerequest = await Mediator.Send(new GetMaintenanceRequestByIdQuery() { Id = id});
          
            if(maintenancerequest.IsSuccess)
            {
                
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = maintenancerequest.Data,message = maintenancerequest.Message });
            }
            return NotFound( new { StatusCode=StatusCodes.Status404NotFound, message = maintenancerequest.Message });   
        }

        [HttpGet("external-requests/by-ids")]
            public async Task<IActionResult> GetExternalRequestsByIds([FromQuery] string ids)
            {
                if (string.IsNullOrWhiteSpace(ids))
                {
                    return BadRequest(new ApiResponseDTO<List<GetExternalRequestByIdDto>>
                    {
                        IsSuccess = false,
                        Message = "No IDs provided.",
                        Data = new List<GetExternalRequestByIdDto>()
                    });
                }

                var parsedIds = ids
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.TryParse(id, out var parsed) ? parsed : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                if (!parsedIds.Any())
                {
                     return BadRequest(new
                      {
                         StatusCode = StatusCodes.Status400BadRequest,
                        IsSuccess = false,
                        Message = "No valid IDs found.",
                        Data = new List<GetExternalRequestByIdDto>()
                    });
                }
                

                var query = new GetExternalRequestsByIdsQuery
                {
                    Ids = parsedIds
                };

                var result = await Mediator.Send(query);

               // return Ok(result);

                 return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    message = "External requests retrieved successfully.",
                    errors = "",
                    data = result.Data
                });
            }

        [HttpPost("create")]
        public async Task<ActionResult<ApiResponseDTO<GetMaintenanceRequestDto>>> Create([FromBody] CreateMaintenanceRequestCommand command)
        {
             var validationResult = await _createmaintenancecommandvalidator.ValidateAsync(command);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await Mediator.Send(command);

                        if (response.IsSuccess)
                {
                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status201Created,
                        message = response.Message,
                        errors = "",
                        data = response.Data
                    });
                }

                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = response.Message,
                    errors = ""
                });
        }   
            // [HttpPost("create-WO-from-external-request/by-ids")]
            // public async Task<ActionResult> CreateFromExternalRequest(CreateExternalRequestWorkOrderCommand command)
            // {
            //     var response = await Mediator.Send(command);
             
            //     if (!response.IsSuccess)
            //     {
            //         return BadRequest(response);
            //     }    
            //     return Ok(response);
            // }
            [HttpPost("create-WO-from-external-request/by-ids")]
            public async Task<ActionResult> CreateFromExternalRequest(CreateExternalRequestWorkOrderCommand command)
            {
                var response = await Mediator.Send(command);

                if (!response.IsSuccess)
                {
                    return BadRequest(new
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        message = response.Message,
                        errors = "",
                        data = response.Data // Optional: can be null or empty list
                    });
                }

                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = response.Message,
                    errors = "",
                    data = response.Data
                });
            }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateMaintenanceRequestCommand command)
        {
            var validationResult = await _updateMaintenanceRequestCommandValidator.ValidateAsync(command); // Line 105?
            
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Validation failed",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
                });
            }

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

        [HttpGet("GetExistingVendor/{oldUnitId}/{VendorCode}")]
        public async Task<IActionResult> GetExistingVendor(string oldUnitId, string VendorCode)
        {
            if (oldUnitId == null)
            {
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid OldUnitId" });
            }
            if (VendorCode =="0")
            {
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid VendorCode" });
            }

            var result = await Mediator.Send(new GetExistingVendorDetailsQuery { OldUnitCode = oldUnitId,VendorCode = VendorCode });

            if (result == null || !result.IsSuccess || result.Data == null)
            {
                return NotFound(new { StatusCode = StatusCodes.Status404NotFound, Message = "No Vendor details found" });
            }

            return Ok(new { StatusCode = StatusCodes.Status200OK, Data = result.Data });
        }


        [HttpPatch("status-Closed/{id}")]
            public async Task<IActionResult> UpdateMaintenanceRequestStatus([FromRoute] int id)
            {
                var command = new UpdateMaintenanceRequestStatusCommand
                {
                    Id = id
                };

                var result = await Mediator.Send(command);

                if (!result.IsSuccess)
                    return NotFound(new
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message =  $" Cannot proceed: The Work Order linked to Maintenance Request ID {id} is still in 'Open' status.."
                        
                    });

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = result.Message,
                    Data = result.Data
                });
            }
            [HttpGet("RequestType")]
                public async Task<IActionResult> GetMaintenanceStatusDescAsync()
                {
                    var result = await Mediator.Send(new GetMaintenanceRequestTypeQuery());
                    if (result == null || result.Data == null || result.Data.Count == 0)
                    {
                        return NotFound(new
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            message = "No RequestType  found."
                        });
                    }
                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        message = "RequestType  fetched successfully.",
                        data = result.Data
                    });
                }

                [HttpGet("ServiceType")]
                public async Task<IActionResult> GetMaintenanceServiceDescAsync()
                {
                    var result = await Mediator.Send(new GetMaintenanceServiceTypeQuery());
                    if (result == null || result.Data == null || result.Data.Count == 0)
                    {
                        return NotFound(new
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            message = "No ServiceType Status found."
                        });
                    }
                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        message = "ServiceType  fetched successfully.",
                        data = result.Data
                    });
                }

                [HttpGet("ServiceLocation")]
                public async Task<IActionResult> GetMaintenanceServiceLocationDescAsync()
                {
                    var result = await Mediator.Send(new GetMaintenanceServiceLocationQuery());
                    if (result == null || result.Data == null || result.Data.Count == 0)
                    {
                        return NotFound(new
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            message = "No ServiceLocation  found."
                        });
                    }
                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        message = "ServiceLocation  fetched successfully.",
                        data = result.Data
                    });
                }

                 [HttpGet("SparesType")]
                public async Task<IActionResult> GetMaintenanceSparesTypeDescAsync()
                {
                    var result = await Mediator.Send(new GetMaintenanceSparesTypeQuery());
                    if (result == null || result.Data == null || result.Data.Count == 0)
                    {
                        return NotFound(new
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            message = "No SparesType  found."
                        });
                    }
                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        message = "SparesType  fetched successfully.",
                        data = result.Data
                    });
                }

                 [HttpGet("DispatchMode")]
                public async Task<IActionResult> GetMaintenanceDispatchModeDescAsync()
                {
                    var result = await Mediator.Send(new GetMaintenanceDispatchModeQuery());
                    if (result == null || result.Data == null || result.Data.Count == 0)
                    {
                        return NotFound(new
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            message = "No DispatchMode  found."
                        });
                    }
                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        message = "DispatchMode  fetched successfully.",
                        data = result.Data
                    });
                }

             

      
        
    }
}