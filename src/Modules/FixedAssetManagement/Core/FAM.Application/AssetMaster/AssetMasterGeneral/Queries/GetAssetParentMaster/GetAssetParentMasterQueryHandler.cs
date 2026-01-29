using AutoMapper;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetParentMaster
{
    public class GetAssetParentMasterQueryHandler : IRequestHandler<GetAssetParentMasterQuery, List<AssetMasterGeneralAutoCompleteDTO>>
    {
        private readonly IAssetMasterGeneralQueryRepository _assetMasterRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        
        public GetAssetParentMasterQueryHandler(IAssetMasterGeneralQueryRepository assetMasterRepository,  IMapper mapper, IMediator mediator)
        {
            _assetMasterRepository = assetMasterRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
  
        public async Task<List<AssetMasterGeneralAutoCompleteDTO>> Handle(GetAssetParentMasterQuery request, CancellationToken cancellationToken)
        {
            if (request.AssetType == "Dependent Parent")
            {
                var result = await _assetMasterRepository.GetByAssetNameAsync("");
                if (result is null || result.Count == 0)
                {
                    throw new ValidationException("No Asset found matching the search pattern.");
                   
                }
                var assetMasterDto = _mapper.Map<List<AssetMasterGeneralAutoCompleteDTO>>(result);
                
                // Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAutoComplete",
                    actionCode: "",        
                    actionName: request.AssetType ?? string.Empty,                
                    details: $"Asset '{request.AssetType}' was searched",
                    module: "AssetMasterGeneral"
                );
                await _mediator.Publish(domainEvent, cancellationToken);

                return assetMasterDto;   
            } 
        throw new ValidationException("Invalid AssetType. Only 'Dependent Parent' is supported.");
          
        }
    }
}
