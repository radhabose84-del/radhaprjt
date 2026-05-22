using AutoMapper;
using Contracts;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Queries.GetAllSalesOrder
{
    public class GetAllSalesOrderQueryHandler : IRequestHandler<GetAllSalesOrderQuery, ApiResponseDTO<List<SalesOrderHeaderDto>>>
    {
        private readonly ISalesOrderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IAccessPolicyService _accessPolicyService;

        public GetAllSalesOrderQueryHandler(
            ISalesOrderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator,
            IAccessPolicyService accessPolicyService)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _accessPolicyService = accessPolicyService;
        }

        public async Task<ApiResponseDTO<List<SalesOrderHeaderDto>>> Handle(GetAllSalesOrderQuery request, CancellationToken cancellationToken)
        {
            var allowedTypeIds = await _accessPolicyService.GetAllowedValueIdsAsync(
                AccessPolicyCodes.SalesOrderType, cancellationToken);

            // Policy configured but user has no allowed type values — return empty without hitting DB
            if (allowedTypeIds != null && allowedTypeIds.Count == 0)
            {
                return new ApiResponseDTO<List<SalesOrderHeaderDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<SalesOrderHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }

            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm,
                request.OrderDateFrom, request.OrderDateTo, request.PartyName, request.StatusName,
                request.SalesOrderTypeMasterId, allowedTypeIds);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesOrderQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Sales Order details were fetched.",
                module: "SalesOrder");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesOrderHeaderDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
