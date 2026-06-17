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

namespace FinanceManagement.Application.TaxCode.Queries.GetPendingTaxAccountLinkage
{
    public class GetPendingTaxAccountLinkageQueryHandler : IRequestHandler<GetPendingTaxAccountLinkageQuery, ApiResponseDTO<List<PendingTaxAccountLinkageDto>>>
    {
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _userLookup;
        private readonly IMediator _mediator;

        public GetPendingTaxAccountLinkageQueryHandler(
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

        public async Task<ApiResponseDTO<List<PendingTaxAccountLinkageDto>>> Handle(GetPendingTaxAccountLinkageQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetPendingLinkagesAsync(request.PageNumber, request.PageSize, request.SearchTerm, companyId);

            if (data.Count > 0)
            {
                // Workflow filtering: only show change requests where the current user is the pending approver
                // (mirrors GetPendingSalesOrderQueryHandler.GetApproverListAsync).
                var currentUserId = _ipAddressService.GetUserId();
                var moduleIds = data.Select(r => r.Id).Distinct().ToList();

                var wfApprovers = await _workflowLookup.GetApproverListAsync(MiscEnumEntity.TaxAccountLinkage, moduleIds);

                var allowedIds = wfApprovers
                    .Where(a => int.TryParse(a.ApproverValue, out var uid) && uid == currentUserId)
                    .Select(a => a.ModuleTransactionId)
                    .ToHashSet();

                data = data.Where(r => allowedIds.Contains(r.Id)).ToList();
                totalCount = data.Count;

                // Approver details: resolve the pending approver user id(s) per row → name(s) from AppSecurity.Users.
                if (data.Count > 0)
                {
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
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPendingTaxAccountLinkageQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Pending tax-account linkage details were fetched.",
                module: "TaxAccountLinkage"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<PendingTaxAccountLinkageDto>>
            {
                IsSuccess = true,
                Message = "Pending tax-account linkage list retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
