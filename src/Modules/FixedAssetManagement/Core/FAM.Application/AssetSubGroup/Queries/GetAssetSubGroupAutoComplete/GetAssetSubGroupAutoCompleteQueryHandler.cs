using AutoMapper;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetSubGroup.Queries.GetAssetSubGroupAutoComplete
{
    public class GetAssetSubGroupAutoCompleteQueryHandler: IRequestHandler<GetAssetSubGroupAutoCompleteQuery,List<AssetSubGroupAutoCompleteDTO>>
    {
        private readonly IAssetSubGroupQueryRepository _iAssetSubGroupQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetSubGroupAutoCompleteQueryHandler(IAssetSubGroupQueryRepository iAssetSubGroupQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetSubGroupQueryRepository = iAssetSubGroupQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<AssetSubGroupAutoCompleteDTO>> Handle(GetAssetSubGroupAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetSubGroupQueryRepository.GetAssetSubGroups(request.SearchPattern??string.Empty);
            var assetSubGroups = _mapper.Map<List<AssetSubGroupAutoCompleteDTO>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",        
                    actionName: "",
                    details: $"AssetSubGroup details was fetched.",
                    module:"AssetSubGroup"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return assetSubGroups;
        }
    }
}