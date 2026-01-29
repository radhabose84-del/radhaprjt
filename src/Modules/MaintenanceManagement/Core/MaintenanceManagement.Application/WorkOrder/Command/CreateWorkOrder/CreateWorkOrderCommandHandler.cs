

// using AutooMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces;
// using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
// using MaintenanceManagement.Application.Common.RealTimeNotificationHub;
// using MaintenanceManagement.Domain.Events;
// using MediatR;
// using Microsoft.AspNetCore.SignalR;
// using Serilog;

// namespace MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder
// {
//     public class CreateWorkOrderCommandHandler : IRequestHandler<CreateWorkOrderCommand, ApiResponseDTO<WorkOrderCombineDto>>
//     {
//         private readonly IMapper _mapper;
//         private readonly IWorkOrderCommandRepository _workOrderRepository;
//         private readonly IWorkOrderQueryRepository _workOrderQueryRepository;
//         private readonly IMediator _mediator;
//         private readonly IIPAddressService _ipAddressService;
//         private readonly IUnitGrpcClient _unitGrpcClient; 
//         private readonly ICompanyGrpcClient _companyGrpcClient;         

//         public CreateWorkOrderCommandHandler(IMapper mapper, IWorkOrderCommandRepository workOrderRepository, IWorkOrderQueryRepository workOrderQueryRepository, IMediator mediator, IIPAddressService ipAddressService, IUnitGrpcClient unitGrpcClient,ICompanyGrpcClient companyGrpcClient)
//         {
//             _mapper = mapper;
//             _workOrderRepository = workOrderRepository;
//             _workOrderQueryRepository = workOrderQueryRepository;
//             _mediator = mediator;     
//             _ipAddressService = ipAddressService;    
//             _unitGrpcClient = unitGrpcClient;
//             _companyGrpcClient=companyGrpcClient;            
//         }

//         public async Task<ApiResponseDTO<WorkOrderCombineDto>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
//         {            
//             var woEntity = _mapper.Map<Core.Domain.Entities.WorkOrderMaster.WorkOrder>(request.WorkOrderDto);               

//             var companyId = _ipAddressService.GetCompanyId();
//             var unitId = _ipAddressService.GetUnitId();
      
//             woEntity.CompanyId = companyId; 
//             woEntity.UnitId = unitId; 
//             woEntity.TotalManPower=0;
//             woEntity.TotalSpentHours=0;    
//             woEntity.CreatedByName=     _ipAddressService.GetUserName();   
//             woEntity.CreatedBy=      int.Parse(_ipAddressService.GetCurrentUserId());
//             woEntity.CreatedIP=     _ipAddressService.GetSystemIPAddress();
            
//             var result = await _workOrderRepository.CreateAsync(woEntity,request.WorkOrderDto.RequestTypeId, cancellationToken);
//             //Domain Event
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "Create",
//                 actionCode: "",
//                 actionName: woEntity.WorkOrderDocNo??string.Empty,
//                 details: $"WorkOrder '{woEntity.WorkOrderDocNo}' was created",
//                 module: "WorkOrder"
//             );
//             await _mediator.Publish(domainEvent, cancellationToken);
        
//             var woMasterDTO = _mapper.Map<WorkOrderCombineDto>(result);
//             if (result.Id > 0)
//             {    
//                 // Notify clients via SignalR       
//                 string tempFilePath = request.WorkOrderDto.Image;
//                 if (tempFilePath != null){
//                     string baseDirectory = await _workOrderQueryRepository.GetBaseDirectoryAsync();
//                     //GRPC
//                     var units = await _unitGrpcClient.GetAllUnitAsync();
//                     var companies = await _companyGrpcClient.GetAllCompanyAsync();
//                     var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);
//                     var companyLookup = companies.ToDictionary(u => u.CompanyId, u => u.CompanyName);
//                     string? unitName = null;
//                     string? companyName = null;

//                     if (unitLookup.TryGetValue(unitId, out var unitNameGrpc))
//                     {
//                         unitName= unitNameGrpc;
//                     }
//                     if (companyLookup.TryGetValue(companyId, out var companyNameGrpc))
//                     {
//                         companyName= companyNameGrpc;
//                     }
                        
//                     //var (companyName, unitName) = await _workOrderRepository.GetCompanyUnitAsync(companyId, unitId);

//                     string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory,companyName,unitName);                      
//                     string filePath = Path.Combine(uploadPath, tempFilePath);
//                     EnsureDirectoryExists(Path.GetDirectoryName(filePath)); 
                       
//                     if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
//                     {
//                         string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
//                         string newFileName = $"{woEntity.WorkOrderDocNo}{Path.GetExtension(tempFilePath)}";
//                         string newFilePath = Path.Combine(directory, newFileName);

//                         try
//                         {
//                             File.Move(filePath, newFilePath);
//                             //assetEntity.AssetImage = newFileName;
//                             await _workOrderRepository.UpdateWOImageAsync(woEntity.Id, newFileName);
//                         }
//                         catch (Exception ex)
//                         {
//                             Log.Information(ex, "Failed to rename file: {Message}", ex.Message);
//                         }
//                     }                    
//                 }
//                 return new ApiResponseDTO<WorkOrderCombineDto>
//                 {
//                     IsSuccess = true,
//                     Message = "Work Order created successfully.",
//                     Data = woMasterDTO
//                 };
//             }
//             return new ApiResponseDTO<WorkOrderCombineDto>
//             {
//                 IsSuccess = false,
//                 Message = "Work Order not created."
//             };
//         }   
//         private void EnsureDirectoryExists(string path)
//         {
//             if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
//             {
//                 Directory.CreateDirectory(path);
//             }
//         }         
//     }
// }