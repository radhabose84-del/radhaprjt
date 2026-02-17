using System.Text.Json;
using AutoMapper;
using PurchaseManagement.Application.Common.HttpResponse;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndent
{
    public class GetPendingIndentQueryHandler : IRequestHandler<GetPendingIndentQuery, ApiResponseDTO<List<PendingIndentDto>>>
    {
        private readonly IPurchaseIndentQuery _purchaseIndentQuery;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IUnitLookup _unitLookup;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUserLookup _usersAllLookup;
        private readonly IIPAddressService _ipAddressService;
        public GetPendingIndentQueryHandler(IPurchaseIndentQuery purchaseIndentQuery, IMediator mediator, IMapper mapper, IUnitLookup unitLookup,
            IWorkflowLookup workflowLookup, IDepartmentLookup departmentLookup, IUserLookup usersLookup, IIPAddressService ipAddressService)
        {
            _purchaseIndentQuery = purchaseIndentQuery;
            _mediator = mediator;
            _mapper = mapper;
            _unitLookup = unitLookup;
            _workflowLookup = workflowLookup;
            _departmentLookup = departmentLookup;
            _usersAllLookup = usersLookup;
            _ipAddressService = ipAddressService;
        }
          public async Task<ApiResponseDTO<List<PendingIndentDto>>> Handle(GetPendingIndentQuery request, CancellationToken cancellationToken)
        {


            var (Indent, TotalCount) = await _purchaseIndentQuery.GetPendingPurchaseIndentAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var IndentDto = _mapper.Map<List<PendingIndentDto>>(Indent);

            var Units = await _unitLookup.GetAllUnitAsync();
            var UnitLookup = Units.ToDictionary(d => d.UnitId, d => d.UnitName);
            var departmentData = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departmentData.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            var indentIds = IndentDto.Select(d => d.Id).ToList();
            var workflowApproverResponse = await _workflowLookup.GetApproverListAsync(MiscEnumEntity.PurchaseIndent,indentIds);
            var ApproverLookup = workflowApproverResponse.ToDictionary(d => d.ModuleTransactionId, d => d.ApproverValue);
            var ApprovalRequestHeaderIdLookup = workflowApproverResponse.ToDictionary(d => d.ModuleTransactionId, d => d.ApprovalRequestId);
            
            var IsEditLookup = workflowApproverResponse.ToDictionary(d => d.ModuleTransactionId, d => d.IsEdit);

            foreach (var dto in IndentDto)
            {
                if (UnitLookup.TryGetValue(dto.UnitId, out var UnitName))
                {
                    dto.UnitName = UnitName;
                }
                if (departmentLookup.TryGetValue(dto.DepartmentId, out var DepartmentName))
                {
                    dto.DepartmentName = DepartmentName;
                }
                if (ApprovalRequestHeaderIdLookup.TryGetValue(dto.Id, out var ApprovalRequestHeaderId))
                {
                    dto.ApprovalRequestHeaderId = Convert.ToInt32(ApprovalRequestHeaderId);
                }

                if (ApproverLookup.TryGetValue(dto.Id, out var ApproverValue))
                {
                    dto.ApproverId = Convert.ToInt32(ApproverValue);
                }

                 // ✅ Set IsEdit from workflow
                if (IsEditLookup.TryGetValue(dto.Id, out var isEditValue))
                {
                    // 👉 If IsEdit in workflow is BYTE:
                    // dto.IsEdit = isEditValue;

                    // 👉 If IsEdit in workflow is BOOL:
                    dto.IsEdit = isEditValue;
                }




            }
            var approverNameMap = await _usersAllLookup.GetAllUserAsync();
            var approverNameLookup = approverNameMap.ToDictionary(d => d.UserId, d => d.UserName);
            foreach (var dto in IndentDto)
            {
                if (approverNameLookup.TryGetValue(dto.ApproverId, out var UserName))
                {
                    dto.ApproverName = UserName;
                }
            }

            var FilteredIndent = IndentDto
                .Where(p => UnitLookup.ContainsKey(p.UnitId))
                .Where(p => p.ApproverId == _ipAddressService.GetUserId())
                .ToList();

            // var workflowResponse = await _workflowGrpcClient.GetAllApprovalRequestStatusAsync(MiscEnumEntity.PurchaseIndent);
            // var workflowLookup = workflowResponse.ToDictionary(d => d.ModuleTransactionId, d => d.CurrentStatus);

            // foreach (var statusMap in FilteredIndent)
            // {
            //     if (workflowLookup.TryGetValue(statusMap.Id, out var Status))
            //     {
            //         statusMap.Status = Status;
            //     }
            // }

            //     var FilteredIndentByPending = FilteredIndent
            // .Where(p => workflowLookup.ContainsKey(p.Id))
            // .ToList();
        
        var evt = new AuditLogsDomainEvent(
                actionDetail: "GetPendingIndent",
                actionCode: "GetPendingIndent",
                actionName: "GetPendingIndent",
                details: JsonSerializer.Serialize(request),
                module: "PurchaseIndent"
            );
            await _mediator.Publish(evt, cancellationToken);

            return new ApiResponseDTO<List<PendingIndentDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = FilteredIndent ?? new List<PendingIndentDto>(),
                TotalCount = TotalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}