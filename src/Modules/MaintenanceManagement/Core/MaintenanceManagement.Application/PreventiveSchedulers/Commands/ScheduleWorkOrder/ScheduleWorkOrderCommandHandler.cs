using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Domain.Common;
using MediatR;
using Newtonsoft.Json;
using static MaintenanceManagement.Domain.Common.MiscEnumEntity;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.ScheduleWorkOrder
{
    public class ScheduleWorkOrderCommandHandler : IRequestHandler<ScheduleWorkOrderCommand, ApiResponseDTO<bool>>
    {
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IWorkOrderCommandRepository _workOrderRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IPreventiveScheduleLogService _preventiveScheduleLogService;
        public ScheduleWorkOrderCommandHandler(IPreventiveSchedulerQuery preventiveSchedulerQuery, IMapper mapper, IMediator mediator,
        IMiscMasterQueryRepository miscMasterQueryRepository, IWorkOrderCommandRepository workOrderRepository, IIPAddressService iPAddressService, IPreventiveScheduleLogService preventiveScheduleLogService)
        {
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
            _mapper = mapper;
            _mediator = mediator;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _workOrderRepository = workOrderRepository;
            _ipAddressService = iPAddressService;
            _preventiveScheduleLogService = preventiveScheduleLogService;
        }
        public async Task<ApiResponseDTO<bool>> Handle(ScheduleWorkOrderCommand request, CancellationToken cancellationToken)
        {
            
            var miscdetail = await _miscMasterQueryRepository.GetMiscMasterByName(WOStatus.MiscCode,StatusOpen.Code);
         //   var ExistItems = await _preventiveSchedulerQuery.ExistPreventivescheduleItem(request.PreventiveScheduleId);
           // var scheduledetail;
           
                 var scheduledetail = await _preventiveSchedulerQuery.GetWorkOrderScheduleDetailById(request.PreventiveScheduleId);
            
            
        await _preventiveScheduleLogService.CaptureLogs(scheduledetail.Id,request.PreventiveScheduleId,"Hangfire tigger Schedule Work Order",JsonConvert.SerializeObject(request));
        
             await AuditLogPublisher.PublishAuditLogAsync(
                    _mediator,
                    actionDetail: $"Schedule Work Order request",
                    actionCode: "Schedule work order",
                    actionName: "Schedule work order",
                    module: "Preventive",
                    requestData: request,
                    cancellationToken
                   );
                
                       var workOrderRequest =  _mapper.Map<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>(scheduledetail, opt =>
                        {
                            opt.Items["StatusId"] = miscdetail.Id;
                            opt.Items["PreventiveSchedulerDetailId"] = scheduledetail.PreventiveSchedulerDetails?.FirstOrDefault()?.Id;   
                        });
                        workOrderRequest.CreatedByName=MiscEnumEntity.System;
                        workOrderRequest.CreatedBy=1;
                        workOrderRequest.CreatedDate=DateTime.Now;
                        workOrderRequest.CreatedIP=_ipAddressService.GetSystemIPAddress() ?? "System";
                        
                        await AuditLogPublisher.PublishAuditLogAsync(
                     _mediator,
                     actionDetail: $"Schedule Work Order creation request MaintenanceCategoryId:{scheduledetail.MaintenanceCategoryId}CompanyId:{scheduledetail.CompanyId}UnitId:{scheduledetail.UnitId}",
                     actionCode: "Schedule work order",
                     actionName: "Schedule work order",
                     module: "Preventive",
                     requestData: workOrderRequest,
                     cancellationToken
                    );

                        var response = await _workOrderRepository.CreatePreventiveAsync(workOrderRequest,scheduledetail.MaintenanceCategoryId,scheduledetail.CompanyId,scheduledetail.UnitId, cancellationToken);
                        if (response.Id == 0)
                        {
                             return new ApiResponseDTO<bool>
                              {
                                  IsSuccess = false, 
                                  Message = "Work Order not create."
                              };
                        }
                          return new ApiResponseDTO<bool>
                              {
                                  IsSuccess = true, 
                                  Message = "Work Order created."
                              };
             
        }
    }
}