using AutoMapper;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralAutoComplete
{
    public class GetAssetMasterGeneralAutoCompleteQueryHandler : IRequestHandler<GetAssetMasterGeneralAutoCompleteQuery, List<AssetMasterGeneralAutoCompleteDTO>>
    {
        private readonly IAssetMasterGeneralQueryRepository _assetMasterRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetAssetMasterGeneralAutoCompleteQueryHandler(IAssetMasterGeneralQueryRepository assetMasterRepository,  IMapper mapper, IMediator mediator)
        {
            _assetMasterRepository =assetMasterRepository;
            _mapper =mapper;
            _mediator = mediator;
        }
        public async Task<List<AssetMasterGeneralAutoCompleteDTO>> Handle(GetAssetMasterGeneralAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _assetMasterRepository.GetByAssetNameAsync(request.SearchPattern ?? string.Empty);
            if (result is null || result.Count is 0)
            {
                throw new ValidationException("No Asset found matching the search pattern."); 
              
            }
            var assetMasterDto = _mapper.Map<List<AssetMasterGeneralAutoCompleteDTO>>(result);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAutoComplete",
                actionCode:"",        
                actionName: request.SearchPattern ?? string.Empty,                
                details: $"Asset '{request.SearchPattern}' was searched",
                module:"AssetMasterGeneral"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return  assetMasterDto;  
        }
    }
}