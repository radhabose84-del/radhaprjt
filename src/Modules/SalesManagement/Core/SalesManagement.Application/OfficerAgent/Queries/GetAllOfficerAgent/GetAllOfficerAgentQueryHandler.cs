using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.OfficerAgent.Queries.GetAllOfficerAgent
{
    public class GetAllOfficerAgentQueryHandler
        : IRequestHandler<GetAllOfficerAgentQuery, ApiResponseDTO<List<OfficerAgentDto>>>
    {
        private readonly IOfficerAgentQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllOfficerAgentQueryHandler(
            IOfficerAgentQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<OfficerAgentDto>>> Handle(
            GetAllOfficerAgentQuery request,
            CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<List<OfficerAgentDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllOfficerAgentQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "OfficerAgent details were fetched.",
                module: "OfficerAgent"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<OfficerAgentDto>>
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
