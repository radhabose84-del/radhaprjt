using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using Contracts.Interfaces;
using MediatR;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetWorkflowTypeAutoComplete
{
    public class GetWorkflowTypeAutoCompleteQueryHandler : IRequestHandler<GetWorkflowTypeAutoCompleteQuery, List<GetWorkflowTypeAutoCompleteDto>>
    {
        private readonly IWorkflowTypeQuery _workflowTypeQuery;
        private readonly IMapper _mapper;
        private readonly ILookupRepository _lookupRepository;
        private readonly IIPAddressService _ipAddressService;

        public GetWorkflowTypeAutoCompleteQueryHandler(
            IWorkflowTypeQuery workflowTypeQuery,
            IMapper mapper,
            ILookupRepository lookupRepository,
            IIPAddressService ipAddressService)
        {
            _workflowTypeQuery = workflowTypeQuery;
            _mapper = mapper;
            _lookupRepository = lookupRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task<List<GetWorkflowTypeAutoCompleteDto>> Handle(GetWorkflowTypeAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var Result = await _workflowTypeQuery.GetWorkflowTypeAutoComplete(request.SearchPattern ?? string.Empty);

            // Filter by role-based user rights — only show menus the current user can view
            var userId = _ipAddressService.GetUserId();
            var accessibleMenuIds = await _lookupRepository.GetUserAccessibleMenuIdsAsync(userId, cancellationToken);
            Result = Result.Where(x => accessibleMenuIds.Contains(x.MenuId)).ToList();

            var WorkflowType = _mapper.Map<List<GetWorkflowTypeAutoCompleteDto>>(Result);

            var menuLookup = await _lookupRepository.GetMenuNamesAsync(
                WorkflowType.Select(x => x.MenuId),
                cancellationToken);

            // Batch-fetch TransactionTypeNames for entries that have a TransactionTypeId
            var txnTypeIds = Result
                .Where(x => x.TransactionTypeId.HasValue)
                .Select(x => x.TransactionTypeId!.Value)
                .Distinct();

            var txnTypeLookup = await _lookupRepository.GetTransactionTypeNamesAsync(txnTypeIds, cancellationToken);

            var txnTypeByWorkflowId = Result
                .Where(x => x.TransactionTypeId.HasValue)
                .ToDictionary(x => x.Id, x => x.TransactionTypeId!.Value);

            foreach (var dto in WorkflowType)
            {
                if (menuLookup.TryGetValue(dto.MenuId, out var menuName))
                {
                    dto.MenuName = menuName;
                }

                if (txnTypeByWorkflowId.TryGetValue(dto.Id, out var txnTypeId)
                    && txnTypeLookup.TryGetValue(txnTypeId, out var txnTypeName))
                {
                    dto.MenuName = $"{txnTypeName}";
                }
            }

            return WorkflowType;
        }
    }
}
