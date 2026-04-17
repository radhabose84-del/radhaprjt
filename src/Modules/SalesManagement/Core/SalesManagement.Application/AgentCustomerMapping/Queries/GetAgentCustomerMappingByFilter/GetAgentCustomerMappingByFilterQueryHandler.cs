using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.AgentCustomerMapping.Dto;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingByFilter
{
    public class GetAgentCustomerMappingByFilterQueryHandler
        : IRequestHandler<GetAgentCustomerMappingByFilterQuery, ApiResponseDTO<List<AgentCustomerMappingDto>>>
    {
        private readonly IAgentCustomerMappingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAgentCustomerMappingByFilterQueryHandler(
            IAgentCustomerMappingQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AgentCustomerMappingDto>>> Handle(
            GetAgentCustomerMappingByFilterQuery request,
            CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetByFilterAsync(
                request.SalesGroupId, request.CustomerId, cancellationToken);

            var dtos = _mapper.Map<List<AgentCustomerMappingDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByFilter",
                actionCode: "GetAgentCustomerMappingByFilterQuery",
                actionName: dtos.Count.ToString(),
                details: $"AgentCustomerMapping records filtered by SalesGroupId={request.SalesGroupId}, CustomerId={request.CustomerId}. Count: {dtos.Count}.",
                module: "AgentCustomerMapping"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<AgentCustomerMappingDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtos,
                TotalCount = totalCount
            };
        }
    }
}
