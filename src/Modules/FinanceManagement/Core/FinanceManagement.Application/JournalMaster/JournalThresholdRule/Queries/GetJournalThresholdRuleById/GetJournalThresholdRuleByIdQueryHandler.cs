using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetJournalThresholdRuleById
{
    public class GetJournalThresholdRuleByIdQueryHandler : IRequestHandler<GetJournalThresholdRuleByIdQuery, JournalThresholdRuleDto?>
    {
        private readonly IJournalThresholdRuleQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetJournalThresholdRuleByIdQueryHandler(IJournalThresholdRuleQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<JournalThresholdRuleDto?> Handle(GetJournalThresholdRuleByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<JournalThresholdRuleDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetJournalThresholdRuleByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Journal threshold rule details {dto.Id} was fetched.",
                module: "JournalThresholdRule"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
