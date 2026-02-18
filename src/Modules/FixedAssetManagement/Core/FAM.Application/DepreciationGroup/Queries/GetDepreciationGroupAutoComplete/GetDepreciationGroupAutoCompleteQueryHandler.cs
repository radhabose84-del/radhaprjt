#nullable disable
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.DepreciationGroup.Queries.GetDepreciationGroupAutoComplete
{
    public class GetDepreciationGroupAutoCompleteQueryHandler : IRequestHandler<GetDepreciationGroupAutoCompleteQuery, List<DepreciationGroupAutoCompleteDTO>>
    {
        private readonly IDepreciationGroupQueryRepository _depreciationGroupRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetDepreciationGroupAutoCompleteQueryHandler(IDepreciationGroupQueryRepository depreciationGroupRepository,  IMapper mapper, IMediator mediator)
        {
            _depreciationGroupRepository =depreciationGroupRepository;
            _mapper =mapper;
            _mediator = mediator;
        }

        public async Task<List<DepreciationGroupAutoCompleteDTO>> Handle(GetDepreciationGroupAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _depreciationGroupRepository.GetByDepreciationNameAsync(request.SearchPattern ?? string.Empty);
            if (result is null)
                throw new EntityNotFoundException("DepreciationGroup", request.SearchPattern);
          
            var depreciationGroupsDto = _mapper.Map<List<DepreciationGroupAutoCompleteDTO>>(result);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAutoComplete",
                actionCode:"",        
                actionName: request.SearchPattern ?? string.Empty,                
                details: $"Depreciation Group '{request.SearchPattern}' was searched",
                module:"DepreciationGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return depreciationGroupsDto;      
        }
    }
}