#nullable disable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Events.Maintenance;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;

namespace MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder
{
    public class UpdateWorkOrderCommandHandler : IRequestHandler<UpdateWorkOrderCommand, ApiResponseDTO<bool>>
    { 
        private readonly IWorkOrderCommandRepository _workOrderRepository;
        private readonly IWorkOrderQueryRepository _workOrderQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;         
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<UpdateWorkOrderCommandHandler> _logger;        
        private readonly ILogQueryService _logQueryService;
        private readonly IUnitLookup _unitLookup; 
        private readonly ICompanyLookup _companyLookup; 
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IPreventiveScheduleLogService _preventiveScheduleLogService;
        private readonly IPreventiveSchedulerCommand _preventiveSchedulerCommand;

        public UpdateWorkOrderCommandHandler(IWorkOrderCommandRepository workOrderRepository, IMapper mapper, IWorkOrderQueryRepository workOrderQueryRepository,
        IMediator mediator, IEventPublisher eventPublisher, ILogger<UpdateWorkOrderCommandHandler> logger, ILogQueryService logQueryService,
        IUnitLookup unitLookup, ICompanyLookup companyLookup, IHttpContextAccessor httpContextAccessor, ITimeZoneService timeZoneService,
        IPreventiveScheduleLogService preventiveScheduleLogService, IPreventiveSchedulerCommand preventiveSchedulerCommand)
        {
            _workOrderRepository = workOrderRepository;
            _mapper = mapper;
            _workOrderQueryRepository = workOrderQueryRepository;
            _mediator = mediator;
            _eventPublisher = eventPublisher;
            _logger = logger;
            _logQueryService = logQueryService ?? throw new ArgumentNullException(nameof(logQueryService));
            _unitLookup = unitLookup;
            _companyLookup = companyLookup;
            _httpContextAccessor = httpContextAccessor;
            _timeZoneService = timeZoneService;
            _preventiveScheduleLogService = preventiveScheduleLogService;
            _preventiveSchedulerCommand = preventiveSchedulerCommand;
        }

        public async Task<ApiResponseDTO<bool>> Handle(UpdateWorkOrderCommand request, CancellationToken cancellationToken)
        {
            await _preventiveScheduleLogService.CaptureLogs(null,request.WorkOrder.PreventiveScheduleId,"Work Order Update",JsonConvert.SerializeObject(request));
            var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            TimeZoneInfo systemTimeZone;            
            try
            {
                systemTimeZone = TimeZoneInfo.FindSystemTimeZoneById(systemTimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                // Common Windows ↔ IANA mismatch handling
                if (string.Equals(systemTimeZoneId, "India Standard Time", StringComparison.OrdinalIgnoreCase))
                    systemTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
                else
                    systemTimeZone = TimeZoneInfo.Local;
            }

            #pragma warning disable CS8321
            // Helper for nullable DateTimeOffset
            #pragma warning restore CS8321
            #pragma warning disable CS8321
            static DateTimeOffset? ConvertIfHasValue(DateTimeOffset? value, TimeZoneInfo tz)
            #pragma warning restore CS8321
            {
                if (!value.HasValue || value.Value == DateTimeOffset.MinValue)
                    return null;
                return TimeZoneInfo.ConvertTime(value.Value, tz);
            }
       
            var updatedEntity = _mapper.Map<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>(request.WorkOrder);

            _logger.LogInformation("Work Order Status. updatedEntity: {@updatedEntity}",
                       updatedEntity);
            var updateResult = await _workOrderRepository.UpdateAsync(updatedEntity.Id, updatedEntity);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: updatedEntity.WorkOrderDocNo ?? string.Empty,
                actionName: "WorkOrder Update",
                details: $"WorkOrder updated for ID {updatedEntity.Id}",
                module: "WorkOrder"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
            if(updateResult)
            {
               
                var miscMaster = await _workOrderRepository.GetMiscMasterByCodeAsync(MiscEnumEntity.MaintenanceStatusUpdate.Code);

                if (updatedEntity.StatusId == miscMaster.Id && updatedEntity.PreventiveScheduleId.HasValue)
                {                    
                    var NextPreventiveSchedule = await _preventiveSchedulerCommand.CreateNextSchedulerDetailAsync(updatedEntity.PreventiveScheduleId.Value);

                    _logger.LogInformation("Next preventive schedule created. NextPreventiveSchedule: {@NextPreventiveSchedule}",
                       NextPreventiveSchedule);
                        if (NextPreventiveSchedule != null)
                        {
                            var targetDateTime = NextPreventiveSchedule.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue);
                              var delay = targetDateTime - DateTime.Now;

                              var delayInMinutes = (int)delay.TotalMinutes;

                            var correlationId = Guid.NewGuid(); 
                            var @event = new NextSchedulerCreatedEvent
                            {
                                CorrelationId = correlationId,
                                PreventiveSchedulerDetailId = NextPreventiveSchedule.Id,
                                DelayInMinutes = delayInMinutes,
                                WorkOrderId = updatedEntity.Id

                            };

                            await _eventPublisher.SaveEventAsync(@event);
                            await _eventPublisher.PublishPendingEventsAsync();
                        }
                }
                
                
                string tempFilePath = request.WorkOrder.Image;
                if (tempFilePath != null){
                    string baseDirectory = await _workOrderQueryRepository.GetBaseDirectoryAsync();
                       //GRPC
                    var units = await _unitLookup.GetAllUnitAsync();
                    var companies = await _companyLookup.GetAllCompanyAsync();
                    var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);
                    var companyLookup = companies.ToDictionary(u => u.CompanyId, u => u.CompanyName);
                    string unitName = null;
                    string companyName = null;

                    if (unitLookup.TryGetValue(request.WorkOrder.UnitId, out var unitNameGrpc))
                    {
                        unitName= unitNameGrpc;
                    }
                    if (companyLookup.TryGetValue(request.WorkOrder.CompanyId, out var companyNameGrpc))
                    {
                        companyName= companyNameGrpc;
                    } 
                    //var (companyName, unitName) = await _workOrderRepository.GetCompanyUnitAsync(request.WorkOrder.CompanyId, request.WorkOrder.UnitId);
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory,companyName,unitName);     
                    //string companyFolder = Path.Combine(baseDirectory, companyName.Trim());
                    //string unitFolder = Path.Combine(companyFolder,unitName.Trim());
                     string filePath = Path.Combine(uploadPath, tempFilePath);  
                    EnsureDirectoryExists(Path.GetDirectoryName(filePath));           

                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
                        string newFileName = $"{request.WorkOrder.WorkOrderDocNo}{Path.GetExtension(tempFilePath)}";
                        string newFilePath = Path.Combine(directory, newFileName);

                        try
                        {
                            File.Move(filePath, newFilePath);
                            //assetEntity.AssetImage = newFileName;
                            await _workOrderRepository.UpdateWOImageAsync(request.WorkOrder.Id, newFileName);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Failed to rename file: {ErrorMessage}", ex.Message);
                        }
                    }
                }                    
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = true,
                    Message = "WorkOrder updated."                     
                };
            }
            return new ApiResponseDTO<bool>
            {
                IsSuccess = false,
                Message = "WorkOrder not updated."
            };                
        }
           private void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }          
    }
 }
