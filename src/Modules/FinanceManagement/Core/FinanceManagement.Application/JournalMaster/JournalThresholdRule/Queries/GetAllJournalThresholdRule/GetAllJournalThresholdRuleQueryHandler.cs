using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetAllJournalThresholdRule
{
    public class GetAllJournalThresholdRuleQueryHandler : IRequestHandler<GetAllJournalThresholdRuleQuery, ApiResponseDTO<List<JournalThresholdRuleDto>>>
    {
        private readonly IJournalThresholdRuleQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllJournalThresholdRuleQueryHandler(IJournalThresholdRuleQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<JournalThresholdRuleDto>>> Handle(GetAllJournalThresholdRuleQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<List<JournalThresholdRuleDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllJournalThresholdRuleQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Journal threshold rule details were fetched.",
                module: "JournalThresholdRule"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<JournalThresholdRuleDto>>
            {
                IsSuccess = true,
                Message = "Journal threshold rule list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
