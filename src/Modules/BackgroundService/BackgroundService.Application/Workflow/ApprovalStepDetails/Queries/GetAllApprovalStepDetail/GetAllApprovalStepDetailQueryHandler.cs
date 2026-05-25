using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetAllApprovalStepDetail
{
    public class GetAllApprovalStepDetailQueryHandler : IRequestHandler<GetAllApprovalStepDetailQuery, ApiResponseDTO<List<ApprovalStepDetailDto>>>
    {
        private readonly IApprovalStepDetailQuery _approvalStepDetailQuery;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILookupRepository _lookupRepository;
        private readonly IIPAddressService _ipAddressService;

        public GetAllApprovalStepDetailQueryHandler(
            IApprovalStepDetailQuery approvalStepDetailQuery,
            IMediator mediator,
            IMapper mapper,
            ILookupRepository lookupRepository,
            IIPAddressService ipAddressService)
        {
            _approvalStepDetailQuery = approvalStepDetailQuery;
            _mediator = mediator;
            _mapper = mapper;
            _lookupRepository = lookupRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<List<ApprovalStepDetailDto>>> Handle(GetAllApprovalStepDetailQuery request, CancellationToken cancellationToken)
        {
            var (ApprovalStepDetail, TotalCount) = await _approvalStepDetailQuery.GetAllApprovalStepDetailAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            // Filter by role-based user rights — only show menus the current user can view
            var userId = _ipAddressService.GetUserId();
            var accessibleMenuIds = await _lookupRepository.GetUserAccessibleMenuIdsAsync(userId, cancellationToken);
            ApprovalStepDetail = ApprovalStepDetail.Where(x => accessibleMenuIds.Contains(x.WorkflowType.MenuId)).ToList();

            var ApprovalStepDetailDto = _mapper.Map<List<ApprovalStepDetailDto>>(ApprovalStepDetail);

            var menuLookup = await _lookupRepository.GetMenuNamesAsync(ApprovalStepDetailDto.Select(x => x.MenuId), cancellationToken);

            // Batch-fetch TransactionTypeNames for entries that have a TransactionTypeId
            var txnTypeIds = ApprovalStepDetail
                .Where(x => x.WorkflowType.TransactionTypeId.HasValue)
                .Select(x => x.WorkflowType.TransactionTypeId!.Value)
                .Distinct();

            var txnTypeLookup = await _lookupRepository.GetTransactionTypeNamesAsync(txnTypeIds, cancellationToken);

            var txnTypeByDetailId = ApprovalStepDetail
                .Where(x => x.WorkflowType.TransactionTypeId.HasValue)
                .ToDictionary(x => x.Id, x => x.WorkflowType.TransactionTypeId!.Value);

            foreach (var dto in ApprovalStepDetailDto)
            {
                if (menuLookup.TryGetValue(dto.MenuId, out var menuName))
                {
                    dto.MenuName = menuName;
                }

                if (txnTypeByDetailId.TryGetValue(dto.Id, out var txnTypeId)
                    && txnTypeLookup.TryGetValue(txnTypeId, out var txnTypeName))
                {
                    //dto.TransactionTypeName = txnTypeName;
                    dto.MenuName = $"{txnTypeName}";
                }
            }

            return new ApiResponseDTO<List<ApprovalStepDetailDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = ApprovalStepDetailDto,
                TotalCount = TotalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
