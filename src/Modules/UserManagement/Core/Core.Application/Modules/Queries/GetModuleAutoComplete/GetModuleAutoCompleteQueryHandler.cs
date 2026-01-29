using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IModule;
using Core.Application.Modules.Queries.GetModules;
using Core.Domain.Events;
using MediatR;

namespace Core.Application.Modules.Queries.GetModuleAutoComplete
{
    public class GetModuleAutoCompleteQueryHandler : IRequestHandler<GetModuleAutoCompleteQuery,List<ModuleAutoCompleteDTO>>
    {
        private readonly IModuleQueryRepository _moduleQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetModuleAutoCompleteQueryHandler(IModuleQueryRepository moduleQueryRepository, IMapper mapper, IMediator mediator)
        {
            _moduleQueryRepository = moduleQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<ModuleAutoCompleteDTO>> Handle(GetModuleAutoCompleteQuery request, CancellationToken cancellationToken)
        {
             
             
            var result = await _moduleQueryRepository.GetModule(request.SearchPattern);
            var module = _mapper.Map<List<ModuleAutoCompleteDTO>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetModule",
                    actionCode: "",        
                    actionName: "",
                    details: $"Module details was fetched.",
                    module:"Module"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return module;  
        }
    }
}