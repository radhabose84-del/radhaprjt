using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.OfficerAgent.Queries.GetOfficerAgentById
{
    public class GetOfficerAgentByIdQueryHandler
        : IRequestHandler<GetOfficerAgentByIdQuery, ApiResponseDTO<OfficerAgentGroupedDto>>
    {
        private readonly IOfficerAgentQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetOfficerAgentByIdQueryHandler(
            IOfficerAgentQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<OfficerAgentGroupedDto>> Handle(
            GetOfficerAgentByIdQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return new ApiResponseDTO<OfficerAgentGroupedDto>
                {
                    IsSuccess = false,
                    Message = "Marketing Officer not found."
                };

            var dto = _mapper.Map<OfficerAgentGroupedDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetOfficerAgentByIdQuery",
                actionName: dto.MarketingOfficerId.ToString(),
                details: $"OfficerAgent details for officer Id {dto.MarketingOfficerId} was fetched.",
                module: "OfficerAgent"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<OfficerAgentGroupedDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dto
            };
        }
    }
}
