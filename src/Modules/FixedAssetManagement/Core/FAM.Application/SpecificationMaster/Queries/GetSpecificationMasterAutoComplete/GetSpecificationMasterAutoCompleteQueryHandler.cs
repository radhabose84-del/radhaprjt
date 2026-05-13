using AutoMapper;
using FAM.Application.Common.Interfaces.ISpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.SpecificationMaster.Queries.GetSpecificationMasterAutoComplete
{
    public class GetSpecificationMasterAutoCompleteQueryHandler : IRequestHandler<GetSpecificationMasterAutoCompleteQuery, List<SpecificationMasterAutoCompleteDTO>>
    {
        private readonly ISpecificationMasterQueryRepository _specificationMasterRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetSpecificationMasterAutoCompleteQueryHandler(ISpecificationMasterQueryRepository specificationMasterRepository,  IMapper mapper, IMediator mediator)
        {
            _specificationMasterRepository =specificationMasterRepository;
            _mapper =mapper;
            _mediator = mediator;
        }

        public async Task<List<SpecificationMasterAutoCompleteDTO>> Handle(GetSpecificationMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _specificationMasterRepository.GetBySpecificationNameAsync(request.AssetGroupId, request.SearchPattern ?? string.Empty)
                         ?? new List<SpecificationMasters>();

            var specificationMasterDto = _mapper.Map<List<SpecificationMasterAutoCompleteDTO>>(result);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAutoComplete",
                actionCode:"",        
                actionName: request.SearchPattern ?? string.Empty,                
                details: $"SpecificationMaster '{request.SearchPattern}' was searched",
                module:"SpecificationMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return  specificationMasterDto;          
        }      
    }
}