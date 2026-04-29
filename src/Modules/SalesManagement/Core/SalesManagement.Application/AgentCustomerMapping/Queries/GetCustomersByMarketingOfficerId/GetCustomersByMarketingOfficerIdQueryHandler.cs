using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.AgentCustomerMapping.Dto;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCustomerMapping.Queries.GetCustomersByMarketingOfficerId
{
    public class GetCustomersByMarketingOfficerIdQueryHandler
        : IRequestHandler<GetCustomersByMarketingOfficerIdQuery, ApiResponseDTO<List<AgentCustomerMappingDto>>>
    {
        private readonly IAgentCustomerMappingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCustomersByMarketingOfficerIdQueryHandler(
            IAgentCustomerMappingQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AgentCustomerMappingDto>>> Handle(
            GetCustomersByMarketingOfficerIdQuery request,
            CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByMarketingOfficerIdAsync(request.MarketingOfficerId, cancellationToken);

            var dtos = _mapper.Map<List<AgentCustomerMappingDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByMarketingOfficerId",
                actionCode: "GetCustomersByMarketingOfficerIdQuery",
                actionName: request.MarketingOfficerId.ToString(),
                details: $"AgentCustomerMapping records for MarketingOfficerId {request.MarketingOfficerId} were fetched. Count: {dtos.Count}.",
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
