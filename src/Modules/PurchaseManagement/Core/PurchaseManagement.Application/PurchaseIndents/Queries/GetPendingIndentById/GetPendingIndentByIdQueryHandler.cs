using System.Text.Json;
using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Workflow;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndentById
{
    public class GetPendingIndentByIdQueryHandler : IRequestHandler<GetPendingIndentByIdQuery, PendingIndentByIdDto>
    {
        private readonly IPurchaseIndentQuery _purchaseIndentQuery;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _usersAllLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uOMLookup;
        private readonly IInventoryCategoryLookup _inventoryCategoryLookup;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;
        public GetPendingIndentByIdQueryHandler(IPurchaseIndentQuery purchaseIndentQuery, IMediator mediator, IMapper mapper,
        IWorkflowLookup workflowLookup, IUserLookup usersAllLookup, IIPAddressService ipAddressService, IItemLookup itemLookup,
        IUOMLookup uOMLookup, IInventoryCategoryLookup inventoryCategoryLookup, IDepartmentLookup departmentLookup, IUnitLookup unitLookup)
        {
            _purchaseIndentQuery = purchaseIndentQuery;
            _mediator = mediator;
            _mapper = mapper;
            _workflowLookup = workflowLookup;
            _usersAllLookup = usersAllLookup;
            _ipAddressService = ipAddressService;
            _itemLookup = itemLookup;
            _uOMLookup = uOMLookup;
            _inventoryCategoryLookup = inventoryCategoryLookup;
            _departmentLookup = departmentLookup;
            _unitLookup = unitLookup;
        }
        public async Task<PendingIndentByIdDto> Handle(GetPendingIndentByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _purchaseIndentQuery.GetByIdAsync(request.Id);     
               
            var Indent = _mapper.Map<PendingIndentByIdDto>(result);

            var indentIds = new List<int> { Indent.Id };
            var workflowResponse = await _workflowLookup.GetApprovalRequestLineAsync(MiscEnumEntity.PurchaseIndent,indentIds, _ipAddressService.GetUserId());
            var workflowApproverResponse = await _workflowLookup.GetApproverListAsync(MiscEnumEntity.PurchaseIndent,indentIds);

             var ApprovalRequestLineIdLookup = workflowResponse.ToDictionary(d => d.ModuleLineTransactionId, d => d.ApprovalRequestLineTransactionId);
             var ApproverLookup = workflowApproverResponse.ToDictionary(d => d.ModuleTransactionId, d => d.ApproverValue);
             var ApprovalRequestHeaderIdLookup = workflowApproverResponse.ToDictionary(d => d.ModuleTransactionId, d => d.ApprovalRequestId);
             
            var itemIds = Indent.IndentDetails.Select(d => d.ItemId).ToList();
            var itemCategoryIds = Indent.IndentDetails.Select(d => d.ItemCategoryId).ToList();

            var itemdata = await _itemLookup.GetByIdsAsync(itemIds, cancellationToken);
            var UomData = await _uOMLookup.GetAllAsync();
            var categoryData = await _inventoryCategoryLookup.GetCategoryByIdsAsync(itemCategoryIds, cancellationToken);
            var departmentData = await _departmentLookup.GetAllDepartmentAsync();
            var unitData = await _unitLookup.GetAllUnitAsync();

            var itemLookup = itemdata.ToDictionary(d => d.Id, d => d.ItemName);
             var uomLookup = UomData.ToDictionary(d => d.Id, d => d.UOMName);
             var categoryLookup = categoryData.ToDictionary(d => d.Id, d => d.ItemCategoryName);
             var departmentLookup = departmentData.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
             var unitLookup = unitData.ToDictionary(d => d.UnitId, d => d.UnitName);
             
               if (departmentLookup.TryGetValue(Indent.DepartmentId, out var DepartmentName))
                {
                    Indent.DepartmentName = DepartmentName ?? string.Empty;
                }
                 if (unitLookup.TryGetValue(Indent.UnitId, out var UnitName))
                {
                    Indent.UnitName = UnitName;
                }
            
                    if (ApprovalRequestHeaderIdLookup.TryGetValue(Indent.Id, out var ApprovalRequestHeaderId))
                     {
                         Indent.ApprovalRequestHeaderId = Convert.ToInt32(ApprovalRequestHeaderId);
                     }
                     if (ApproverLookup.TryGetValue(Indent.Id, out var ApproverValue))
                    {
                        Indent.ApproverId = Convert.ToInt32(ApproverValue);
                    }


            foreach (var dto in Indent.IndentDetails)
            {
                if (ApprovalRequestLineIdLookup.TryGetValue(dto.Id, out var ApprovalRequestLineId))
                {
                    dto.ApprovalRequestLineId = Convert.ToInt32(ApprovalRequestLineId);
                }
                     if (itemLookup.TryGetValue(dto.ItemId, out var ItemName))
                {
                    dto.ItemName = ItemName;
                }
                if (uomLookup.TryGetValue(dto.ItemUOMId, out var UOMName))
                {
                    dto.UOMName = UOMName;
                }
                if (categoryLookup.TryGetValue(dto.ItemCategoryId, out var CategoryName))
                {
                    dto.ItemCategoryName = CategoryName;
                }

             }

        
            var approverNameMap = await _usersAllLookup.GetAllUserAsync();
            var approverNameLookup = approverNameMap.ToDictionary(d => d.UserId, d => d.UserName);
           
                if (approverNameLookup.TryGetValue(Indent.ApproverId, out var UserName))
                {
                    Indent.ApproverName = UserName ?? string.Empty;
                }
              //  approverMap.IsApprover = approverMap.ApproverId == _ipAddressService.GetUserId() ? "Y" : "N";
               var evt = new AuditLogsDomainEvent(
                actionDetail: "GetPendingIndentById",
                actionCode: "GetPendingIndentById",
                actionName: "GetPendingIndentById",
                details: JsonSerializer.Serialize(request),
                module: "PurchaseIndent"
            );
            await _mediator.Publish(evt, cancellationToken);
            
            return Indent;
        }
    }
}