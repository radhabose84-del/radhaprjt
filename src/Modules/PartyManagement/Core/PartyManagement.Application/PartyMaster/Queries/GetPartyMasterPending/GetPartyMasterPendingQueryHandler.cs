using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
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

                // Build a lookup: ModuleTransactionId -> list of approver entries (multi-level approval)
                var workflowByTransaction = workflowApproverResponse
                    .GroupBy(x => x.ModuleTransactionId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var currentUserId = _ipAddressService.GetUserId();

                foreach (var dto in partyMasterDtos)
                {
                    if (UnitLookup.TryGetValue(dto.UnitId, out var UnitName))
                        dto.UnitName = UnitName;

                    // Find the approval entry for the current user (multi-level: pick the one matching current user)
                    if (workflowByTransaction.TryGetValue(dto.Id, out var approverEntries))
                    {
                        var currentUserEntry = approverEntries
                            .FirstOrDefault(a => a.ApproverValue == currentUserId.ToString())
                            ?? approverEntries.First();

                        dto.ApprovalRequestHeaderId = currentUserEntry.ApprovalRequestId;
                        dto.ApproverId = Convert.ToInt32(currentUserEntry.ApproverValue);
                        dto.IsEdit = currentUserEntry.IsEdit;
                    }
                }

                var approverNameMap = await _usersAllLookup.GetAllUserAsync();
                var approverNameLookup = approverNameMap.ToDictionary(d => d.UserId, d => d.UserName);

                foreach (var dto in partyMasterDtos)
                    if (approverNameLookup.TryGetValue(dto.ApproverId, out var UserName))
                        dto.ApproverName = UserName;

                var FilteredIndent = partyMasterDtos
                    .Where(p => UnitLookup.ContainsKey(p.UnitId))
                    .Where(p => p.ApproverId == currentUserId)
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
