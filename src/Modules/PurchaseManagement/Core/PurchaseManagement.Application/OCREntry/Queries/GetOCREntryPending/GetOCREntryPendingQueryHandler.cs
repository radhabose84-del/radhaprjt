using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.OCREntry.Queries.GetOCREntryPending
{
    public class GetOCREntryPendingQueryHandler : IRequestHandler<GetOCREntryPendingQuery, ApiResponseDTO<List<OCREntryDto>>>
    {
        private readonly IOCREntryQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IUserLookup _userLookup;

        public GetOCREntryPendingQueryHandler(
            IOCREntryQueryRepository queryRepository,
            IMediator mediator,
            IWorkflowLookup workflowLookup,
            IIPAddressService ipAddressService,
            IUserLookup userLookup)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
            _workflowLookup = workflowLookup;
            _ipAddressService = ipAddressService;
            _userLookup = userLookup;
        }

        public async Task<ApiResponseDTO<List<OCREntryDto>>> Handle(GetOCREntryPendingQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetPendingAsync(request.PageNumber, request.PageSize);

            if (data.Count > 0)
            {
                // ---------------- Filter by current user's approver id ----------------
                var currentUserId = _ipAddressService.GetUserId();

                var ocrIds = data.Select(r => r.Id)
                                 .Where(id => id > 0)
                                 .Distinct()
                                 .ToList();

                var wfApprovers = await _workflowLookup
                    .GetApproverListAsync(MiscEnumEntity.TransactionTypeOCR, ocrIds);

                // Allowed OCR ids where the current user is an approver
                var allowedOcrIds = wfApprovers
                    .Where(a => !string.IsNullOrWhiteSpace(a.ApproverValue)
                                && int.TryParse(a.ApproverValue, out var parsed)
                                && parsed == currentUserId)
                    .Select(a => a.ModuleTransactionId)
                    .ToHashSet();

                data = data.Where(r => allowedOcrIds.Contains(r.Id)).ToList();

                if (data.Count > 0)
                {
                    // Workflow map for kept OCRs
                    var wfByModuleId = wfApprovers
                        .Where(a => allowedOcrIds.Contains(a.ModuleTransactionId))
                        .GroupBy(a => a.ModuleTransactionId)
                        .ToDictionary(
                            g => g.Key,
                            g =>
                            {
                                var first = g.FirstOrDefault(x => int.TryParse(x.ApproverValue, out _)) ?? g.First();
                                int? approverId = null;
                                if (!string.IsNullOrWhiteSpace(first.ApproverValue) &&
                                    int.TryParse(first.ApproverValue, out var parsed))
                                {
                                    approverId = parsed;
                                }
                                return new
                                {
                                    ApproverId = approverId,
                                    ApprovalRequestId = first.ApprovalRequestId,
                                    IsEdit = first.IsEdit
                                };
                            });

                    var users = await _userLookup.GetAllUserAsync();
                    var userLookupDict = users.ToDictionary(u => u.UserId, u => u.UserName);

                    foreach (var dto in data)
                    {
                        if (wfByModuleId.TryGetValue(dto.Id, out var wf))
                        {
                            if (wf.ApproverId.HasValue)
                            {
                                dto.ApproverId = wf.ApproverId.Value;
                                if (userLookupDict.TryGetValue(wf.ApproverId.Value, out var approverName))
                                    dto.ApproverName = approverName;
                            }
                            dto.ApprovalRequestHeaderId = wf.ApprovalRequestId;
                            dto.IsEdit = wf.IsEdit;
                        }
                    }
                }
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetOCREntryPendingQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Pending OCR details were fetched.",
                module: "OCREntry");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<OCREntryDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = data.Count,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
