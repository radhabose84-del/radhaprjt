using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCommissionConfig.Queries.GetAllAgentCommissionConfig
{
    public class GetAllAgentCommissionConfigQueryHandler
        : IRequestHandler<GetAllAgentCommissionConfigQuery, ApiResponseDTO<List<AgentCommissionConfigDto>>>
    {
        private readonly IAgentCommissionConfigQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllAgentCommissionConfigQueryHandler(
            IAgentCommissionConfigQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AgentCommissionConfigDto>>> Handle(
            GetAllAgentCommissionConfigQuery request,
            CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<List<AgentCommissionConfigDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllAgentCommissionConfigQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "AgentCommissionConfig details were fetched.",
                module: "AgentCommissionConfig"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<AgentCommissionConfigDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
