using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Application.CommissionSplit.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.CommissionSplit.Queries.GetAllCommissionSplit
{
    public class GetAllCommissionSplitQueryHandler : IRequestHandler<GetAllCommissionSplitQuery, ApiResponseDTO<List<CommissionSplitDto>>>
    {
        private readonly ICommissionSplitQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllCommissionSplitQueryHandler(ICommissionSplitQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<CommissionSplitDto>>> Handle(GetAllCommissionSplitQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllCommissionSplitQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "CommissionSplit details were fetched.",
                module: "CommissionSplit"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<CommissionSplitDto>>
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
