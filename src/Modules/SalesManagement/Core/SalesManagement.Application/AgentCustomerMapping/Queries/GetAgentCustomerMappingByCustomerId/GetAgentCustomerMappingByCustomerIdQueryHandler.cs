using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.AgentCustomerMapping.Dto;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingByCustomerId
{
    public class GetAgentCustomerMappingByCustomerIdQueryHandler
        : IRequestHandler<GetAgentCustomerMappingByCustomerIdQuery, ApiResponseDTO<List<AgentCustomerMappingDto>>>
    {
        private readonly IAgentCustomerMappingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAgentCustomerMappingByCustomerIdQueryHandler(
            IAgentCustomerMappingQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AgentCustomerMappingDto>>> Handle(
            GetAgentCustomerMappingByCustomerIdQuery request,
            CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

            var dtos = _mapper.Map<List<AgentCustomerMappingDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByCustomerId",
                actionCode: "GetAgentCustomerMappingByCustomerIdQuery",
                actionName: request.CustomerId.ToString(),
                details: $"AgentCustomerMapping records for CustomerId {request.CustomerId} were fetched. Count: {dtos.Count}.",
                module: "AgentCustomerMapping"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<AgentCustomerMappingDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtos,
                TotalCount = dtos.Count
            };
        }
    }
}
