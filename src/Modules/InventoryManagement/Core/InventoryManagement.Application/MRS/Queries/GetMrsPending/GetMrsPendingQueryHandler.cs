using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using InventoryManagement.Application.Common.HttpResponse;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.MRS.Queries.GetMrsPending
{
    public class GetMrsPendingQueryHandler : IRequestHandler<GetMrsPendingQuery, ApiResponseDTO<List<MrsPendingDto>>>
    {
        private readonly IMrsEntryQueryRepository _iMrsEntryQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IUnitLookup _unitLookup;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUserLookup _usersAllLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IIPAddressService _ipAddressService;
         public GetMrsPendingQueryHandler(IMrsEntryQueryRepository iMrsEntryQueryRepository, IMediator mediator, IMapper mapper, IUnitLookup unitLookup,
            IWorkflowLookup workflowLookup, IDepartmentLookup departmentLookup, IUserLookup usersAllLookup, IIPAddressService ipAddressService, IUOMLookup uomLookup)
        {
            _iMrsEntryQueryRepository = iMrsEntryQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _unitLookup = unitLookup;
            _workflowLookup = workflowLookup;
            _departmentLookup = departmentLookup;
            _usersAllLookup = usersAllLookup;
            _ipAddressService = ipAddressService;
            _uomLookup = uomLookup;
        
        }

        public async Task<ApiResponseDTO<List<MrsPendingDto>>> Handle(GetMrsPendingQuery request, CancellationToken cancellationToken)
        {
            // Step 1: Get MRS list and total count
            var (Mrs, TotalCount) = await _iMrsEntryQueryRepository.GetPendingMrsDetailsAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm
            );
            var mrsPendingDtos = _mapper.Map<List<MrsPendingDto>>(Mrs);

            // Step 2: Trigger parallel lookup calls
            var unitsTask = _unitLookup.GetAllUnitAsync();
            var departmentTask = _departmentLookup.GetAllDepartmentAsync();
            var usersTask = _usersAllLookup.GetAllUserAsync();
            var uomTask = _uomLookup.GetAllAsync();

            // Step 3: Workflow approvers (depends on MRS list)
            var mrsIds = mrsPendingDtos.Select(d => d.Id).ToList();
            var workflowApproverTask = _workflowLookup.GetApproverListAsync(MiscEnumEntity.MaterialRequest, mrsIds);

            // Step 4: Await all lookups together
            await Task.WhenAll(unitsTask, departmentTask, usersTask, uomTask, workflowApproverTask);

            // Step 5: Convert lookups
            var UnitLookup = (await unitsTask).ToDictionary(u => u.UnitId, u => u.UnitName);
            var DepartmentLookup = (await departmentTask).ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            var UomLookup = (await uomTask).ToDictionary(u => u.Id, u => u.UOMName);
            var workflowApproverResponse = await workflowApproverTask;
            var ApproverLookup = workflowApproverResponse
                .GroupBy(a => a.ModuleTransactionId)
                .ToDictionary(g => g.Key, g => g.First().ApproverValue);

            var ApprovalRequestHeaderIdLookup = workflowApproverResponse
                    .ToDictionary(d => d.ModuleTransactionId, d => d.ApprovalRequestId);
            var ApproverNameLookup = (await usersTask).ToDictionary(u => u.UserId, u => u.UserName);
            
           var IsEditLookup = workflowApproverResponse
            .GroupBy(a => a.ModuleTransactionId)
            .ToDictionary(g => g.Key, g => g.First().IsEdit);

            // Step 6: Enrich DTOs
            foreach (var dto in mrsPendingDtos)
            {
                if (UnitLookup.TryGetValue(dto.UnitId, out var unitName))
                    dto.UnitName = unitName;

                if (DepartmentLookup.TryGetValue(dto.DepartmentId, out var deptName))
                    dto.DepartmentName = deptName;

                if (DepartmentLookup.TryGetValue(dto.SubDepartmentId, out var subDeptName))
                    dto.SubDepartmentName = subDeptName;

                if (ApproverLookup.TryGetValue(dto.Id, out var approverId))
                    dto.ApproverId = Convert.ToInt32(approverId);

                if (ApproverNameLookup.TryGetValue(dto.ApproverId, out var approverName))
                    dto.ApproverName = approverName;

                if (ApprovalRequestHeaderIdLookup.TryGetValue(dto.Id, out var ApprovalRequestHeaderId))
                    dto.ApprovalRequestHeaderId = Convert.ToInt32(ApprovalRequestHeaderId);
                
                if (IsEditLookup.TryGetValue(dto.Id, out var isEdit))
                    dto.IsEdit = isEdit;
                else
                    dto.IsEdit = 0;

                
                foreach (var detail in dto.MrsDetails)
                {
                    if (UomLookup.TryGetValue(detail.UomId, out var uomName))
                        detail.UomName = uomName;
                }
            }

            // Step 7: Optional filtering
            var FilteredIndent = mrsPendingDtos
                .Where(p => UnitLookup.ContainsKey(p.UnitId))
                .Where(p => p.ApproverId == _ipAddressService.GetUserId())
                .ToList();

            // Step 8: Audit log
            var evt = new AuditLogsDomainEvent(
                actionDetail: "GetMrsPendingQuery",
                actionCode: "GetMrsPendingQuery",
                actionName: "GetMrsPendingQuery",
                details: JsonSerializer.Serialize(request),
                module: "MaterialRequest"
            );
            await _mediator.Publish(evt, cancellationToken);

            // Step 9: Response
            return new ApiResponseDTO<List<MrsPendingDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = FilteredIndent,
                TotalCount = TotalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        
    }
}
