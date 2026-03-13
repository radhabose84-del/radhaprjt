using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.AgentCustomerMapping.Dto;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.AgentCustomerMapping.Queries.GetAllAgentCustomerMapping
{
    public class GetAllAgentCustomerMappingQueryHandler
        : IRequestHandler<GetAllAgentCustomerMappingQuery, ApiResponseDTO<List<AgentCustomerMappingDto>>>
    {
        private readonly IAgentCustomerMappingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllAgentCustomerMappingQueryHandler(
            IAgentCustomerMappingQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AgentCustomerMappingDto>>> Handle(
            GetAllAgentCustomerMappingQuery request,
            CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<List<AgentCustomerMappingDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllAgentCustomerMappingQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "AgentCustomerMapping details were fetched.",
                module: "AgentCustomerMapping"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<AgentCustomerMappingDto>>
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
