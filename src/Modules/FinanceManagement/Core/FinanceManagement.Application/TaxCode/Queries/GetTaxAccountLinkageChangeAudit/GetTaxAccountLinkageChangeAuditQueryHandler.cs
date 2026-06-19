using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxAccountLinkageChangeAudit
{
    public class GetTaxAccountLinkageChangeAuditQueryHandler
        : IRequestHandler<GetTaxAccountLinkageChangeAuditQuery, ApiResponseDTO<List<PendingTaxAccountLinkageDto>>>
    {
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _userLookup;
        private readonly IMediator _mediator;

        public GetTaxAccountLinkageChangeAuditQueryHandler(
            ITaxCodeQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IWorkflowLookup workflowLookup,
            IUserLookup userLookup,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _workflowLookup = workflowLookup;
            _userLookup = userLookup;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<PendingTaxAccountLinkageDto>>> Handle(GetTaxAccountLinkageChangeAuditQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetChangeAuditLinkagesAsync(
                request.PageNumber, request.PageSize, request.SearchTerm, companyId, request.StatusId);

            if (data.Count > 0)
            {
                // Resolve Approver 1 (FC) / Approver 2 (Tax) per change request from the Workflow module.
                // (No current-user filter — Change Audit shows every change, unlike the Pending inbox.)
                var moduleIds = data.Select(r => r.Id).Distinct().ToList();
                var wfApprovers = await _workflowLookup.GetApproverListAsync(MiscEnumEntity.TaxAccountLinkage, moduleIds);

                var approverIdsByModule = wfApprovers
                    .Where(a => int.TryParse(a.ApproverValue, out _))
                    .GroupBy(a => a.ModuleTransactionId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(a => int.Parse(a.ApproverValue)).Distinct().ToList());

                var allApproverIds = approverIdsByModule.Values.SelectMany(x => x).Distinct().ToList();
                IReadOnlyList<UserLookupDto> users = allApproverIds.Count > 0
                    ? await _userLookup.GetByIdsAsync(allApproverIds, cancellationToken)
                    : (IReadOnlyList<UserLookupDto>)Array.Empty<UserLookupDto>();
                var nameById = users.ToDictionary(u => u.UserId, u => u.UserName);

                foreach (var row in data)
                {
                    if (!approverIdsByModule.TryGetValue(row.Id, out var ids) || ids.Count == 0)
                        continue;

                    row.Approver1Name = nameById.TryGetValue(ids[0], out var n1) ? n1 : null;
                    if (ids.Count > 1)
                        row.Approver2Name = nameById.TryGetValue(ids[1], out var n2) ? n2 : null;
                }
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetTaxAccountLinkageChangeAuditQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Tax-account linkage change-audit details were fetched.",
                module: "TaxAccountLinkage"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<PendingTaxAccountLinkageDto>>
            {
                IsSuccess = true,
                Message = "Tax-account linkage change-audit list retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
