// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using Contracts.Interfaces.External.IUser;
// using Contracts.Interfaces.External.IWorkflow;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndentById
// {
//     public class GetPendingIndentByIdQueryHandler : IRequestHandler<GetPendingIndentByIdQuery, PendingIndentByIdDto>
//     {
//         private readonly IPurchaseIndentQuery _purchaseIndentQuery;
//         private readonly IMediator _mediator;
//         private readonly IMapper _mapper;
//         private readonly IWorkflowGrpcClient _workflowGrpcClient;
//         private readonly IUsersAllGrpcClient _usersAllGrpcClient;
//         private readonly IIPAddressService _ipAddressService;
//          private readonly IItemGrpcClient _itemGrpcClient;
//         private readonly IUOMGrpcClient _uOMGrpcClient;
//         private readonly IInventoryCategoryGrpcClient _inventoryCategoryGrpcClient;
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
//         private readonly IUnitGrpcClient _unitGrpcClient;
//         public GetPendingIndentByIdQueryHandler(IPurchaseIndentQuery purchaseIndentQuery, IMediator mediator, IMapper mapper,
//         IWorkflowGrpcClient workflowGrpcClient, IUsersAllGrpcClient usersAllGrpcClient, IIPAddressService ipAddressService, IItemGrpcClient itemGrpcClient,
//         IUOMGrpcClient uOMGrpcClient, IInventoryCategoryGrpcClient inventoryCategoryGrpcClient, IDepartmentAllGrpcClient departmentAllGrpcClient, IUnitGrpcClient unitGrpcClient)
//         {
//             _purchaseIndentQuery = purchaseIndentQuery;
//             _mediator = mediator;
//             _mapper = mapper;
//             _workflowGrpcClient = workflowGrpcClient;
//             _usersAllGrpcClient = usersAllGrpcClient;
//             _ipAddressService = ipAddressService;
//             _itemGrpcClient = itemGrpcClient;
//             _uOMGrpcClient = uOMGrpcClient;
//             _inventoryCategoryGrpcClient = inventoryCategoryGrpcClient;
//             _departmentAllGrpcClient = departmentAllGrpcClient;
//             _unitGrpcClient = unitGrpcClient;
//         }
//         public async Task<PendingIndentByIdDto> Handle(GetPendingIndentByIdQuery request, CancellationToken cancellationToken)
//         {
//             var result = await _purchaseIndentQuery.GetByIdAsync(request.Id);     
               
//             var Indent = _mapper.Map<PendingIndentByIdDto>(result);

//             var indentIds = new List<int> { Indent.Id };
//             var workflowResponse = await _workflowGrpcClient.GetApprovalRequestLineAsync(MiscEnumEntity.PurchaseIndent,indentIds, _ipAddressService.GetUserId());
//             var workflowApproverResponse = await _workflowGrpcClient.GetApproverListAsync(MiscEnumEntity.PurchaseIndent,indentIds);

//              var ApprovalRequestLineIdLookup = workflowResponse.ToDictionary(d => d.ModuleLineTransactionId, d => d.ApprovalRequestLineTransactionId);
//              var ApproverLookup = workflowApproverResponse.ToDictionary(d => d.ModuleTransactionId, d => d.ApproverValue);
//              var ApprovalRequestHeaderIdLookup = workflowApproverResponse.ToDictionary(d => d.ModuleTransactionId, d => d.ApprovalRequestId);
             
//             var itemIds = Indent.IndentDetails.Select(d => d.ItemId).ToList();
//             var itemCategoryIds = Indent.IndentDetails.Select(d => d.ItemCategoryId).ToList();

//             var itemdata = await _itemGrpcClient.GetItemsByIdsAsync(itemIds, cancellationToken);
//             var UomData = await _uOMGrpcClient.GetUOMAsync();
//             var categoryData = await _inventoryCategoryGrpcClient.GetCategoryByIdsAsync(itemCategoryIds, cancellationToken);
//             var departmentData = await _departmentAllGrpcClient.GetDepartmentAllAsync();
//             var unitData = await _unitGrpcClient.GetAllUnitAsync();

//             var itemLookup = itemdata.ToDictionary(d => d.Id, d => d.ItemName);
//              var uomLookup = UomData.ToDictionary(d => d.Id, d => d.UOMName);
//              var categoryLookup = categoryData.ToDictionary(d => d.Id, d => d.ItemCategoryName);
//              var departmentLookup = departmentData.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
//              var unitLookup = unitData.ToDictionary(d => d.UnitId, d => d.UnitName);
             
//                if (departmentLookup.TryGetValue(Indent.DepartmentId, out var DepartmentName))
//                 {
//                     Indent.DepartmentName = DepartmentName;
//                 }
//                  if (unitLookup.TryGetValue(Indent.UnitId, out var UnitName))
//                 {
//                     Indent.UnitName = UnitName;
//                 }
            
//                     if (ApprovalRequestHeaderIdLookup.TryGetValue(Indent.Id, out var ApprovalRequestHeaderId))
//                      {
//                          Indent.ApprovalRequestHeaderId = Convert.ToInt32(ApprovalRequestHeaderId);
//                      }
//                      if (ApproverLookup.TryGetValue(Indent.Id, out var ApproverValue))
//                     {
//                         Indent.ApproverId = Convert.ToInt32(ApproverValue);
//                     }


//             foreach (var dto in Indent.IndentDetails)
//             {
//                 if (ApprovalRequestLineIdLookup.TryGetValue(dto.Id, out var ApprovalRequestLineId))
//                 {
//                     dto.ApprovalRequestLineId = Convert.ToInt32(ApprovalRequestLineId);
//                 }
//                      if (itemLookup.TryGetValue(dto.ItemId, out var ItemName))
//                 {
//                     dto.ItemName = ItemName;
//                 }
//                 if (uomLookup.TryGetValue(dto.ItemUOMId, out var UOMName))
//                 {
//                     dto.UOMName = UOMName;
//                 }
//                 if (categoryLookup.TryGetValue(dto.ItemCategoryId, out var CategoryName))
//                 {
//                     dto.ItemCategoryName = CategoryName;
//                 }

//              }

        
//             var approverNameMap = await _usersAllGrpcClient.GetUserAllAsync();
//             var approverNameLookup = approverNameMap.ToDictionary(d => d.UserId, d => d.UserName);
           
//                 if (approverNameLookup.TryGetValue(Indent.ApproverId, out var UserName))
//                 {
//                     Indent.ApproverName =  UserName;
//                 }
//               //  approverMap.IsApprover = approverMap.ApproverId == _ipAddressService.GetUserId() ? "Y" : "N";
//                var evt = new AuditLogsDomainEvent(
//                 actionDetail: "GetPendingIndentById",
//                 actionCode: "GetPendingIndentById",
//                 actionName: "GetPendingIndentById",
//                 details: JsonSerializer.Serialize(request),
//                 module: "PurchaseIndent"
//             );
//             await _mediator.Publish(evt, cancellationToken);
            
//             return Indent;
//         }
//     }
// }