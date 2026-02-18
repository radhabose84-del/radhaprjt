#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Command.CreateExternalRequestWorkOrder
{
    public class CreateExternalRequestWorkOrderCommandHandler : IRequestHandler<CreateExternalRequestWorkOrderCommand, ApiResponseDTO<List<int>>>
    {

        
       private readonly IMaintenanceRequestCommandRepository  _maintenanceRequestCommandRepository;
       private readonly IMapper _imapper;
       private readonly IMediator _mediator;
       private readonly IMaintenanceRequestQueryRepository  _maintenanceRequestQueryRepository;
       private readonly IWorkOrderCommandRepository _workOrderCommandRepository;
         private readonly IWorkOrderQueryRepository _workOrderQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        public CreateExternalRequestWorkOrderCommandHandler(IMaintenanceRequestCommandRepository maintenanceRequestCommandRepository,IMapper mapper,IMediator mediator,IMaintenanceRequestQueryRepository maintenanceRequestQueryRepository,IWorkOrderCommandRepository workOrderCommandRepository,IWorkOrderQueryRepository workOrderQueryQueryRepository,IIPAddressService ipAddressService,ITimeZoneService timeZoneService)
        {
             _maintenanceRequestCommandRepository = maintenanceRequestCommandRepository;
             _imapper = mapper;
             _mediator = mediator;
             _maintenanceRequestQueryRepository = maintenanceRequestQueryRepository;
             _workOrderCommandRepository = workOrderCommandRepository;
             _workOrderQueryRepository = workOrderQueryQueryRepository;
             _ipAddressService = ipAddressService;
             _timeZoneService = timeZoneService;
        }

        public async Task<ApiResponseDTO<List<int>>> Handle(CreateExternalRequestWorkOrderCommand request, CancellationToken cancellationToken)
            {
               
                var externalRequests = await _maintenanceRequestQueryRepository.GetExternalRequestByIdAsync(request.Ids);
                if (externalRequests == null || !externalRequests.Any())
                    {
                        return new ApiResponseDTO<List<int>>
                        {
                            IsSuccess = false,
                            Message = "No external requests found for the given IDs.",
                            Data = new List<int>() // Optional: return empty list
                        };
                    }
                // Fetch the Misc Open Status once
                var statusList = await _maintenanceRequestQueryRepository.GetMaintenanceOpenstatusAsync();
                var openStatus = statusList.FirstOrDefault();
                
                if (openStatus == null)
                {
                    return CreateErrorResponse("Open status not found in MiscMaster.");
                }

                // Process each external request
                foreach (var externalRequest in externalRequests)
                {
                   
            var workOrder = _imapper.Map<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>(externalRequest);

                string currentIp = _ipAddressService.GetSystemIPAddress();
                int userId = _ipAddressService.GetUserId(); 
                string username = _ipAddressService.GetUserName();
                var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
                var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);  

                      workOrder.CreatedBy = userId;
                      workOrder.CreatedDate = currentTime;
                      workOrder.CreatedByName = username;
                      workOrder.CreatedIP = currentIp;
                    var result = await _workOrderCommandRepository.CreateAsync(workOrder,externalRequest.MaintenanceTypeId, cancellationToken);
                   
                }

                return new ApiResponseDTO<List<int>>
                {
                    IsSuccess = true,
                    Message = $" Work Order(s) created successfully from external requests"
                };
            }

            // Utility method to simplify error response creation
            private ApiResponseDTO<List<int>> CreateErrorResponse(string message)
            {
                return new ApiResponseDTO<List<int>>
                {
                    IsSuccess = false,
                    Message = message,
                    Data = new List<int>()
                };
            }


  
                    
        
    }
}