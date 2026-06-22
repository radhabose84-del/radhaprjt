using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.CoaChangeRequest.Dto;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CoaChangeRequest.Queries.GetCoaChangeRequests
{
    public class GetCoaChangeRequestsQueryHandler
        : IRequestHandler<GetCoaChangeRequestsQuery, ApiResponseDTO<List<CoaChangeRequestDto>>>
    {
        private readonly ICoaChangeRequestQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetCoaChangeRequestsQueryHandler(
            ICoaChangeRequestQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<CoaChangeRequestDto>>> Handle(
            GetCoaChangeRequestsQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId() ?? 0;

            var (items, total) = await _queryRepository.GetChangeRequestsAsync(
                companyId, request.Status, request.PageNumber, request.PageSize, cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetCoaChangeRequestsQuery",
                actionName: items.Count.ToString(),
                details: "COA change requests were fetched.",
                module: "CoaChangeRequest"), cancellationToken);

            return new ApiResponseDTO<List<CoaChangeRequestDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = items,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
