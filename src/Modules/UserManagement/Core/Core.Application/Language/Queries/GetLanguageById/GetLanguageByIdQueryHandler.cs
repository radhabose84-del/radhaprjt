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

namespace Core.Application.Language.Queries.GetLanguageById
{
    public class GetLanguageByIdQueryHandler : IRequestHandler<GetLanguageByIdQuery,LanguageDTO>
    {
        private readonly ILanguageQuery _languageQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetLanguageByIdQueryHandler(ILanguageQuery languageQuery, IMapper mapper, IMediator mediator)    
        {
            _languageQuery = languageQuery;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<LanguageDTO> Handle(GetLanguageByIdQuery request, CancellationToken cancellationToken)
        {
             var result = await _languageQuery.GetByIdAsync(request.Id);
             var language = _mapper.Map<LanguageDTO>(result);

          
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",        
                    actionName: "",
                    details: $"Language details {language.Id} was fetched.",
                    module:"Language"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
             return language;
        }
    }
}