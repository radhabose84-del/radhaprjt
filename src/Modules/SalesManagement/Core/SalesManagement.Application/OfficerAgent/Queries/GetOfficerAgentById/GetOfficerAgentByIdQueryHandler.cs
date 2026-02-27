using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.OfficerAgent.Queries.GetOfficerAgentById
{
    public class GetOfficerAgentByIdQueryHandler
        : IRequestHandler<GetOfficerAgentByIdQuery, ApiResponseDTO<OfficerAgentDto>>
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

        public async Task<ApiResponseDTO<OfficerAgentDto>> Handle(
            GetOfficerAgentByIdQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return new ApiResponseDTO<OfficerAgentDto> { IsSuccess = false, Message = "OfficerAgent not found." };

            var dto = _mapper.Map<OfficerAgentDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetOfficerAgentByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"OfficerAgent details {dto.Id} was fetched.",
                module: "OfficerAgent"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<OfficerAgentDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dto
            };
        }
    }
}
