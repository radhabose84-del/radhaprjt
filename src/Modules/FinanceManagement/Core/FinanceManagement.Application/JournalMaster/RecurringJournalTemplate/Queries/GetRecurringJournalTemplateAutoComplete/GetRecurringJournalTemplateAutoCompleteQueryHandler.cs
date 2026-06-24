using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetRecurringJournalTemplateAutoComplete
{
    public class GetRecurringJournalTemplateAutoCompleteQueryHandler : IRequestHandler<GetRecurringJournalTemplateAutoCompleteQuery, IReadOnlyList<RecurringJournalTemplateLookupDto>>
    {
        private readonly IRecurringJournalTemplateQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetRecurringJournalTemplateAutoCompleteQueryHandler(IRecurringJournalTemplateQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<RecurringJournalTemplateLookupDto>> Handle(GetRecurringJournalTemplateAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term, cancellationToken);
            var dtos = _mapper.Map<List<RecurringJournalTemplateLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetRecurringJournalTemplateAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Recurring journal template details was fetched.",
                module: "RecurringJournalTemplate"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
