using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.External.IUser;
using Contracts.Interfaces.External.IWorkflow;
using PurchaseManagement.Application.Common.HttpResponse;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;

namespace PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturn
{
    public class GetPendingIssueReturnQueryHandler : IRequestHandler<GetPendingIssueReturnQuery, ApiResponseDTO<List<PendingIssueReturnDto>>>
    {
        private readonly IIssueQueryCommandRepository _iissueQueryCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IUnitLookup _unitLookup;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IDepartmentLookup _departmentAllLookup;
        private readonly IUserLookup _usersAllLookup;
        private readonly IIPAddressService _ipAddressService;
        public GetPendingIssueReturnQueryHandler(IIssueQueryCommandRepository iissueQueryCommandRepository, IMediator mediator, IMapper mapper
        , IUnitLookup unitLookup,
        IWorkflowLookup workflowLookup, IDepartmentLookup departmentAllLookup, IUserLookup usersAllLookup
        , IIPAddressService ipAddressService)
        {
            _iissueQueryCommandRepository = iissueQueryCommandRepository;
            _mediator = mediator;
            _mapper = mapper;
            _unitLookup = unitLookup;
            _workflowLookup = workflowLookup;
            _departmentAllLookup = departmentAllLookup;
            _usersAllLookup = usersAllLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<List<PendingIssueReturnDto>>> Handle(GetPendingIssueReturnQuery request, CancellationToken cancellationToken)
        {
            var (IssueReturn, TotalCount) = await _iissueQueryCommandRepository.GetPendingIssueReturnAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var IssueReturnDto = _mapper.Map<List<PendingIssueReturnDto>>(IssueReturn);

            var Units = await _unitLookup.GetAllUnitAsync();
            var UnitLookup = Units.ToDictionary(d => d.UnitId, d => d.UnitName);
            var departmentData = await _departmentAllLookup.GetAllDepartmentAsync();
            var departmentLookup = departmentData.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            var indentIds = IssueReturnDto.Select(d => d.Id).ToList();
            var workflowApproverResponse = await _workflowLookup.GetApproverListAsync(MiscEnumEntity.IssueReturn,indentIds);
            var ApproverLookup = workflowApproverResponse.ToDictionary(d => d.ModuleTransactionId, d => d.ApproverValue);

            foreach (var dto in IssueReturnDto)
            {
                if (UnitLookup.TryGetValue(dto.UnitId, out var UnitName))
                {
                    dto.UnitName = UnitName;
                }
                if (departmentLookup.TryGetValue(dto.DepartmentId, out var DepartmentName))
                {
                    dto.DepartmentName = DepartmentName;
                }
                if (ApproverLookup.TryGetValue(dto.Id, out var ApproverValue))
                {
                    dto.ApproverId = Convert.ToInt32(ApproverValue);
                }
            }
             var approverNameMap = await _usersAllLookup.GetAllUserAsync();
            var approverNameLookup = approverNameMap.ToDictionary(d => d.UserId, d => d.UserName);
            foreach (var dto in IssueReturnDto)
            {
                if (approverNameLookup.TryGetValue(dto.ApproverId, out var UserName))
                {
                    dto.ApproverName = UserName;
                }
            }

            var FilteredIndent = IssueReturnDto
        .Where(p => UnitLookup.ContainsKey(p.UnitId))
        .Where(p => p.ApproverId == _ipAddressService.GetUserId())
        .ToList();
        
        var evt = new AuditLogsDomainEvent(
                actionDetail: "GetPendingIssueReturnQuery",
                actionCode: "GetPendingIssueReturnQuery",
                actionName: "GetPendingIssueReturnQuery",
                details: JsonSerializer.Serialize(request),
                module: "IssueReturn"
            );
            await _mediator.Publish(evt, cancellationToken);

            return new ApiResponseDTO<List<PendingIssueReturnDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = FilteredIndent ?? new List<PendingIssueReturnDto>(),
                TotalCount = TotalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}