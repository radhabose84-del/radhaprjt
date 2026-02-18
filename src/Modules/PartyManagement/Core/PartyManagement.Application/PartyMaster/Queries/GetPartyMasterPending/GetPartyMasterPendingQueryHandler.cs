using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using Contracts.Common;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Domain.Common;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartyMasterPending
{
    public class GetPartyMasterPendingQueryHandler : IRequestHandler<GetPartyMasterPendingQuery, ApiResponseDTO<List<PartyMasterPendingDto>>>
    {
        private readonly IPartyMasterQueryRepository _ipartyMasterQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IUnitLookup _unitLookup;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _usersAllLookup;
        private readonly IIPAddressService _ipAddressService;

        public GetPartyMasterPendingQueryHandler(IPartyMasterQueryRepository ipartyMasterQueryRepository, IMediator mediator, IMapper mapper, IUnitLookup unitLookup,
            IWorkflowLookup workflowLookup, IUserLookup usersAllLookup, IIPAddressService ipAddressService)
        {
            _ipartyMasterQueryRepository = ipartyMasterQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _unitLookup = unitLookup;
            _workflowLookup = workflowLookup;
            _usersAllLookup = usersAllLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<List<PartyMasterPendingDto>>> Handle(GetPartyMasterPendingQuery request, CancellationToken cancellationToken)
        {
            // Call repo (pass dummy values or overload method)
                var (PartyMaster, totalCount) = await _ipartyMasterQueryRepository
                    .GetAllPartyMasterPendingAsync(request.SearchTerm); // <- new overload without paging

                var partyMasterDtos = _mapper.Map<List<PartyMasterPendingDto>>(PartyMaster);

                // ✅ Same enrichment logic (UnitName, Approver, etc.)
                var Units = await _unitLookup.GetAllUnitAsync();
                var UnitLookup = Units.ToDictionary(d => d.UnitId, d => d.UnitName);

                var partyIds = partyMasterDtos.Select(d => d.Id).ToList();
                var workflowApproverResponse = await _workflowLookup
                    .GetApproverListAsync(MiscEnumEntity.PartyDocumentImage.PartyMaster, partyIds);

                var ApproverLookup = workflowApproverResponse
                    .ToDictionary(d => d.ModuleTransactionId, d => d.ApproverValue);
                var ApprovalRequestHeaderIdLookup = workflowApproverResponse
                    .ToDictionary(d => d.ModuleTransactionId, d => d.ApprovalRequestId);
                    
                var IsEditLookup = workflowApproverResponse
                    .GroupBy(x => x.ModuleTransactionId)
                    .ToDictionary(g => g.Key, g => g.First().IsEdit);

            foreach (var dto in partyMasterDtos)
            {
                if (UnitLookup.TryGetValue(dto.UnitId, out var UnitName))
                    dto.UnitName = UnitName;

                if (ApprovalRequestHeaderIdLookup.TryGetValue(dto.Id, out var ApprovalRequestHeaderId))
                    dto.ApprovalRequestHeaderId = Convert.ToInt32(ApprovalRequestHeaderId);

                if (ApproverLookup.TryGetValue(dto.Id, out var ApproverValue))
                    dto.ApproverId = Convert.ToInt32(ApproverValue);
                    
                    if (IsEditLookup.TryGetValue(dto.Id, out var isEdit))
                    dto.IsEdit = isEdit;
                else
                    dto.IsEdit = 0;
            }

                var approverNameMap = await _usersAllLookup.GetAllUserAsync();
                var approverNameLookup = approverNameMap.ToDictionary(d => d.UserId, d => d.UserName);

                foreach (var dto in partyMasterDtos)
                    if (approverNameLookup.TryGetValue(dto.ApproverId, out var UserName))
                        dto.ApproverName = UserName;

                var FilteredIndent = partyMasterDtos
                    .Where(p => UnitLookup.ContainsKey(p.UnitId))
                    .Where(p => p.ApproverId == _ipAddressService.GetUserId())
                    .ToList();

                await _mediator.Publish(new AuditLogsDomainEvent(
                    actionDetail: "GetPartyMasterPendingQuery",
                    actionCode: "Get",
                    actionName: PartyMaster.Count().ToString(),
                    details: "PartyMaster pending details were fetched.",
                    module: "PartyMaster"), cancellationToken);

                return new ApiResponseDTO<List<PartyMasterPendingDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = FilteredIndent,
                    TotalCount = totalCount
                };
        }



    }
}
