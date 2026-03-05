using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.OfficerAgent.Queries.GetAllOfficerAgent
{
    public class GetAllOfficerAgentQueryHandler
        : IRequestHandler<GetAllOfficerAgentQuery, ApiResponseDTO<List<OfficerAgentGroupedDto>>>
    {
        private readonly IOfficerAgentQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllOfficerAgentQueryHandler(
            IOfficerAgentQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<OfficerAgentGroupedDto>>> Handle(
            GetAllOfficerAgentQuery request,
            CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllOfficerAgentQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "OfficerAgent details were fetched.",
                module: "OfficerAgent"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<OfficerAgentGroupedDto>>
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
