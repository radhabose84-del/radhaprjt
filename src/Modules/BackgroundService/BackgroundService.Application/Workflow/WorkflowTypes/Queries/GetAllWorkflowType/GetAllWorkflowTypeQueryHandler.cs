using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using MediatR;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetAllWorkflowType
{
    public class GetAllWorkflowTypeQueryHandler : IRequestHandler<GetAllWorkflowTypeQuery, ApiResponseDTO<List<WorkflowTypeDto>>>
    {
        private readonly IWorkflowTypeQuery _workflowTypeQuery;
        private readonly IMapper _mapper;
        private readonly ILookupRepository _lookupRepository;
        private readonly IIPAddressService _ipAddressService;

        public GetAllWorkflowTypeQueryHandler(
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

        public async Task<ApiResponseDTO<List<WorkflowTypeDto>>> Handle(GetAllWorkflowTypeQuery request, CancellationToken cancellationToken)
        {
            var (WorkflowType, TotalCount) = await _workflowTypeQuery.GetAllWorkflowTypeAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            // Filter by role-based user rights — only show menus the current user can view
            var userId = _ipAddressService.GetUserId();
            var accessibleMenuIds = await _lookupRepository.GetUserAccessibleMenuIdsAsync(userId, cancellationToken);
            WorkflowType = WorkflowType.Where(x => accessibleMenuIds.Contains(x.MenuId)).ToList();

            var WorkflowTypeDto = _mapper.Map<List<WorkflowTypeDto>>(WorkflowType);

            var menuLookup = await _lookupRepository.GetMenuNamesAsync(
                WorkflowTypeDto.Select(x => x.MenuId),
                cancellationToken);

            // Batch-fetch TransactionTypeNames for entries that have a TransactionTypeId
            var txnTypeIds = WorkflowType
                .Where(x => x.TransactionTypeId.HasValue)
                .Select(x => x.TransactionTypeId!.Value)
                .Distinct();

            var txnTypeLookup = await _lookupRepository.GetTransactionTypeNamesAsync(txnTypeIds, cancellationToken);

            var txnTypeByWorkflowId = WorkflowType
                .Where(x => x.TransactionTypeId.HasValue)
                .ToDictionary(x => x.Id, x => x.TransactionTypeId!.Value);

            foreach (var dto in WorkflowTypeDto)
            {
                if (menuLookup.TryGetValue(dto.MenuId, out var menuName))
                {
                    dto.MenuName = menuName;
                }

                if (txnTypeByWorkflowId.TryGetValue(dto.Id, out var txnTypeId)
                    && txnTypeLookup.TryGetValue(txnTypeId, out var txnTypeName))
                {
                    //dto.TransactionTypeName = txnTypeName;
                    dto.MenuName = $"{txnTypeName}";
                }
            }

            return new ApiResponseDTO<List<WorkflowTypeDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = WorkflowTypeDto,
                TotalCount = TotalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
