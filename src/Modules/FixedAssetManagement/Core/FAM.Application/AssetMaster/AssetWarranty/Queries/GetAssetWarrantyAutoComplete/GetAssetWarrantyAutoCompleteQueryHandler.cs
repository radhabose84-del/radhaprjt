using AutoMapper;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarrantyAutoComplete
{
    public class GetAssetWarrantyAutoCompleteQueryHandler  : IRequestHandler<GetAssetWarrantyAutoCompleteQuery, List<AssetWarrantyAutoCompleteDTO>>
        {
            private readonly IAssetWarrantyQueryRepository _assetWarrantyRepository;
            private readonly IMapper _mapper;
            private readonly IMediator _mediator; 

            public GetAssetWarrantyAutoCompleteQueryHandler(IAssetWarrantyQueryRepository assetWarrantyRepository,  IMapper mapper, IMediator mediator)
            {
                _assetWarrantyRepository =assetWarrantyRepository;
                _mapper =mapper;
                _mediator = mediator;
            }

            public async Task<List<AssetWarrantyAutoCompleteDTO>> Handle(GetAssetWarrantyAutoCompleteQuery request, CancellationToken cancellationToken)
            {
                var result = await _assetWarrantyRepository.GetByAssetWarrantyNameAsync(request.SearchPattern ?? string.Empty);
                if (result is null || result.Count is 0)
                {
                    throw new ValidationException("No WarrantyMaster found matching the search pattern.");
                 
                }
                var WarrantyMasterDto = _mapper.Map<List<AssetWarrantyAutoCompleteDTO>>(result);
                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAutoComplete",
                    actionCode:"",        
                    actionName: request.SearchPattern ?? string.Empty,                
                    details: $"Asset Warranty '{request.SearchPattern}' was searched",
                    module:"Asset Warranty"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
                return  WarrantyMasterDto;          
            }      
        }
    }