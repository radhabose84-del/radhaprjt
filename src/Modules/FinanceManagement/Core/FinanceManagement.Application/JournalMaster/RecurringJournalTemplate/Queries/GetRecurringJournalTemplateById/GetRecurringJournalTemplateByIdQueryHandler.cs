using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetRecurringJournalTemplateById
{
    public class GetRecurringJournalTemplateByIdQueryHandler : IRequestHandler<GetRecurringJournalTemplateByIdQuery, RecurringJournalTemplateHeaderDto?>
    {
        private readonly IRecurringJournalTemplateQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetRecurringJournalTemplateByIdQueryHandler(IRecurringJournalTemplateQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<RecurringJournalTemplateHeaderDto?> Handle(GetRecurringJournalTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<RecurringJournalTemplateHeaderDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetRecurringJournalTemplateByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Recurring journal template details {dto.Id} was fetched.",
                module: "RecurringJournalTemplate"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
