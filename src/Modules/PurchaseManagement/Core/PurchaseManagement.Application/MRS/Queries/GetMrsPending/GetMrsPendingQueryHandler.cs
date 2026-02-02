// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using Contracts.Interfaces.External.IUser;
// using Contracts.Interfaces.External.IWorkflow;
// using PurchaseManagement.Application.Common.HttpResponse;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IMRS;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.MRS.Queries.GetMrsPending
// {
//     public class GetMrsPendingQueryHandler : IRequestHandler<GetMrsPendingQuery, ApiResponseDTO<List<MrsPendingDto>>>
//     {
//         private readonly IMrsEntryQueryRepository _iMrsEntryQueryRepository;
//         private readonly IMediator _mediator;
//         private readonly IMapper _mapper;
//         private readonly IUnitGrpcClient _unitGrpcClient;
//         private readonly IWorkflowGrpcClient _workflowGrpcClient;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
//         private readonly IUsersAllGrpcClient _usersAllGrpcClient;
//         private readonly IIPAddressService _ipAddressService;
//         private readonly IUOMGrpcClient _uOMGrpcClient;

//         public GetMrsPendingQueryHandler(IMrsEntryQueryRepository iMrsEntryQueryRepository, IMediator mediator, IMapper mapper, IUnitGrpcClient unitGrpcClient,
//             IWorkflowGrpcClient workflowGrpcClient, IDepartmentAllGrpcClient departmentAllGrpcClient, IUsersAllGrpcClient usersAllGrpcClient, IIPAddressService ipAddressService, IUOMGrpcClient uOMGrpcClient)
//         {
//             _iMrsEntryQueryRepository = iMrsEntryQueryRepository;
//             _mediator = mediator;
//             _mapper = mapper;
//             _unitGrpcClient = unitGrpcClient;
//             _workflowGrpcClient = workflowGrpcClient;
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//             _usersAllGrpcClient = usersAllGrpcClient;
//             _ipAddressService = ipAddressService;
//             _uOMGrpcClient = uOMGrpcClient;
//         }

//         public async Task<ApiResponseDTO<List<MrsPendingDto>>> Handle(GetMrsPendingQuery request, CancellationToken cancellationToken)
//         {
//             // Step 1: Get MRS list and total count
//             var (Mrs, TotalCount) = await _iMrsEntryQueryRepository.GetPendingMrsDetailsAsync(
//                 request.PageNumber,
//                 request.PageSize,
//                 request.SearchTerm
//             );
//             var mrsPendingDtos = _mapper.Map<List<MrsPendingDto>>(Mrs);

//             // Step 2: Trigger parallel GRPC calls
//             var unitsTask = _unitGrpcClient.GetAllUnitAsync();
//             var departmentTask = _departmentAllGrpcClient.GetDepartmentAllAsync();
//             var uomTask = _uOMGrpcClient.GetUOMAsync();
//             var usersTask = _usersAllGrpcClient.GetUserAllAsync();

//             // Step 3: Workflow approvers (depends on MRS list)
//             var mrsIds = mrsPendingDtos.Select(d => d.Id).ToList();
//             var workflowApproverResponse = await _workflowGrpcClient.GetApproverListAsync(MiscEnumEntity.MaterialRequest, mrsIds);

//             // Step 4: Await all lookups together
//             await Task.WhenAll(unitsTask, departmentTask, uomTask, usersTask);

//             // Step 5: Convert lookups
//             var UnitLookup = unitsTask.Result.ToDictionary(u => u.UnitId, u => u.UnitName);
//             var DepartmentLookup = departmentTask.Result.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
//             var UomLookup = uomTask.Result.ToDictionary(u => u.Id, u => u.UOMName);
//             var ApproverLookup = workflowApproverResponse
//                 .GroupBy(a => a.ModuleTransactionId)
//                 .ToDictionary(g => g.Key, g => g.First().ApproverValue);

//             var ApprovalRequestHeaderIdLookup = workflowApproverResponse
//                     .ToDictionary(d => d.ModuleTransactionId, d => d.ApprovalRequestId);
//             var ApproverNameLookup = usersTask.Result.ToDictionary(u => u.UserId, u => u.UserName);
            
//            var IsEditLookup = workflowApproverResponse
//             .GroupBy(a => a.ModuleTransactionId)
//             .ToDictionary(g => g.Key, g => g.First().IsEdit);

//             // Step 6: Enrich DTOs
//             foreach (var dto in mrsPendingDtos)
//             {
//                 if (UnitLookup.TryGetValue(dto.UnitId, out var unitName))
//                     dto.UnitName = unitName;

//                 if (DepartmentLookup.TryGetValue(dto.DepartmentId, out var deptName))
//                     dto.DepartmentName = deptName;

//                 if (DepartmentLookup.TryGetValue(dto.SubDepartmentId, out var subDeptName))
//                     dto.SubDepartmentName = subDeptName;

//                 if (ApproverLookup.TryGetValue(dto.Id, out var approverId))
//                     dto.ApproverId = Convert.ToInt32(approverId);

//                 if (ApproverNameLookup.TryGetValue(dto.ApproverId, out var approverName))
//                     dto.ApproverName = approverName;

//                 if (ApprovalRequestHeaderIdLookup.TryGetValue(dto.Id, out var ApprovalRequestHeaderId))
//                     dto.ApprovalRequestHeaderId = Convert.ToInt32(ApprovalRequestHeaderId);
                
//                 if (IsEditLookup.TryGetValue(dto.Id, out var isEdit))
//                     dto.IsEdit = isEdit;
//                 else
//                     dto.IsEdit = 0;

//                 // ✅ Map UoM name for each detail
//                 foreach (var detail in dto.MrsDetails)
//                 {
//                     if (UomLookup.TryGetValue(detail.UomId, out var uomName))
//                         detail.UomName = uomName;
//                 }
//             }

//             // Step 7: Optional filtering
//             var FilteredIndent = mrsPendingDtos
//                 .Where(p => UnitLookup.ContainsKey(p.UnitId))
//                 .Where(p => p.ApproverId == _ipAddressService.GetUserId())
//                 .ToList();

//             // Step 8: Audit log
//             var evt = new AuditLogsDomainEvent(
//                 actionDetail: "GetMrsPendingQuery",
//                 actionCode: "GetMrsPendingQuery",
//                 actionName: "GetMrsPendingQuery",
//                 details: JsonSerializer.Serialize(request),
//                 module: "MaterialRequest"
//             );
//             await _mediator.Publish(evt, cancellationToken);

//             // Step 9: Response
//             return new ApiResponseDTO<List<MrsPendingDto>>
//             {
//                 IsSuccess = true,
//                 Message = "Success",
//                 Data = FilteredIndent,
//                 TotalCount = TotalCount,
//                 PageNumber = request.PageNumber,
//                 PageSize = request.PageSize
//             };
//         }

//     }
// }