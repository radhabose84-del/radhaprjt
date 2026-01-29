using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.ILanguage;
using Core.Application.Language.Queries.GetLanguages;
using Core.Domain.Events;
using MediatR;

namespace Core.Application.Language.Queries.GetLanguageAutoComplete
{
    public class GetLanguageAutoCompleteQueryHandler : IRequestHandler<GetLanguageAutoCompleteQuery,List<LanguageAutoCompleteDTO>>
    {
        private readonly ILanguageQuery _languageQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetLanguageAutoCompleteQueryHandler(IMapper mapper, ILanguageQuery languageQuery, IMediator mediator)
        {
            _mapper = mapper;
            _languageQuery = languageQuery;
            _mediator = mediator;
        }

        public async Task<List<LanguageAutoCompleteDTO>> Handle(GetLanguageAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _languageQuery.GetLanguage(request.SearchPattern);
            var languages = _mapper.Map<List<LanguageAutoCompleteDTO>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAutoComplete",
                    actionCode: "",        
                    actionName: "",
                    details: $"Language details was fetched.",
                    module:"Language"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return languages;   
        }
    }
}